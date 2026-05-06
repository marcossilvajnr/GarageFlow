using GarageFlow.Domain.Executions;
using GarageFlow.Infrastructure.Observability;
using GarageFlow.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GarageFlow.Tests.Infrastructure.Observability;

public sealed class PersistenceObservabilitySaveChangesInterceptorTests
{
    [Fact]
    public async Task ShouldLogEntityCreationAndStatusTransition()
    {
        var loggerProvider = new ListLoggerProvider();
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddProvider(loggerProvider));
        services.AddDbContext<GarageFlowDbContext>((serviceProvider, options) =>
            options
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .AddInterceptors(new PersistenceObservabilitySaveChangesInterceptor(
                    serviceProvider.GetRequiredService<ILogger<PersistenceObservabilitySaveChangesInterceptor>>())));

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();

        var executionOrder = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        dbContext.ExecutionOrders.Add(executionOrder);
        await dbContext.SaveChangesAsync();

        executionOrder.MarkReadyToStart();
        await dbContext.SaveChangesAsync();

        loggerProvider.Messages.Should().Contain(message =>
            message.Contains("entity_created", StringComparison.Ordinal)
            && message.Contains("aggregate=ExecutionOrder", StringComparison.Ordinal));

        loggerProvider.Messages.Should().Contain(message =>
            message.Contains("state_transition", StringComparison.Ordinal)
            && message.Contains("aggregate=ExecutionOrder", StringComparison.Ordinal)
            && message.Contains("oldState=Pending", StringComparison.Ordinal)
            && message.Contains("newState=Ready", StringComparison.Ordinal));
    }

    private sealed class ListLoggerProvider : ILoggerProvider
    {
        private readonly List<string> _messages = [];

        public IReadOnlyList<string> Messages => _messages;

        public ILogger CreateLogger(string categoryName) => new ListLogger(_messages);

        public void Dispose() { }

        private sealed class ListLogger(List<string> messages) : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                messages.Add(formatter(state, exception));
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
