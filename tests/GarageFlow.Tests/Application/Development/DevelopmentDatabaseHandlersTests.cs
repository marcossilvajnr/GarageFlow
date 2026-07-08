using FluentAssertions;
using GarageFlow.Application.Development.Commands;
using GarageFlow.Application.Development.Handlers;

namespace GarageFlow.Tests.Application.Development;

public sealed class DevelopmentDatabaseHandlersTests
{
    private const string DestructiveOperationBlockedDetail =
        "Operacao destrutiva bloqueada. Envie { \"confirm\": true } para prosseguir.";

    [Fact]
    public async Task MigrateDevelopmentDatabaseHandler_DelegatesToService()
    {
        var service = new FakeDevelopmentDatabaseService();
        var handler = new MigrateDevelopmentDatabaseHandler(service);

        var result = await handler.HandleAsync(new MigrateDevelopmentDatabaseCommand());

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Migrations aplicadas com sucesso.");
        result.Provider.Should().Be("Fake.Provider");
        service.MigrateCalls.Should().Be(1);
    }

    [Fact]
    public async Task CleanDevelopmentDatabaseHandler_WithConfirmation_DelegatesToService()
    {
        var service = new FakeDevelopmentDatabaseService();
        var handler = new CleanDevelopmentDatabaseHandler(service);

        var result = await handler.HandleAsync(new CleanDevelopmentDatabaseCommand(true));

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Banco removido com sucesso.");
        service.CleanCalls.Should().Be(1);
    }

    [Fact]
    public async Task CleanDevelopmentDatabaseHandler_WithoutConfirmation_ReturnsValidationFailure()
    {
        var service = new FakeDevelopmentDatabaseService();
        var handler = new CleanDevelopmentDatabaseHandler(service);

        var result = await handler.HandleAsync(new CleanDevelopmentDatabaseCommand(false));

        result.IsSuccess.Should().BeFalse();
        result.ValidationDetail.Should().Be(DestructiveOperationBlockedDetail);
        service.CleanCalls.Should().Be(0);
    }

    [Fact]
    public async Task ResetDevelopmentDatabaseHandler_WithConfirmation_DelegatesToService()
    {
        var service = new FakeDevelopmentDatabaseService();
        var handler = new ResetDevelopmentDatabaseHandler(service);

        var result = await handler.HandleAsync(new ResetDevelopmentDatabaseCommand(true));

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Banco recriado com sucesso.");
        result.Provider.Should().Be("Fake.Provider");
        service.ResetCalls.Should().Be(1);
    }

    [Fact]
    public async Task ResetDevelopmentDatabaseHandler_WithoutConfirmation_ReturnsValidationFailure()
    {
        var service = new FakeDevelopmentDatabaseService();
        var handler = new ResetDevelopmentDatabaseHandler(service);

        var result = await handler.HandleAsync(new ResetDevelopmentDatabaseCommand(false));

        result.IsSuccess.Should().BeFalse();
        result.ValidationDetail.Should().Be(DestructiveOperationBlockedDetail);
        service.ResetCalls.Should().Be(0);
    }
}
