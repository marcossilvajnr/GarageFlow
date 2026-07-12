using FluentAssertions;
using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.Handlers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;
using GarageFlow.Tests.Application.Parts;
using GarageFlow.Tests.Application.Services;
using GarageFlow.Tests.Application.Supplies;
using Microsoft.Extensions.Logging;
using AppQuoteStatus = GarageFlow.Application.ServiceOrders.Enums.QuoteStatus;

namespace GarageFlow.Tests.Application.ServiceOrders;

public sealed class HandleExternalQuoteDecisionHandlerTests
{
    private static readonly string ValidPartCode = "PRT-EXT-001";
    private static readonly string ValidPartSku = "SKU-EXT-001";
    private static readonly string ValidSupplyCode = "INS-EXT-001";

    // ── Happy paths ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExternalApproval_ReturnsApprovedQuoteDto()
    {
        var (soRepo, order, logger) = await SetupOrderWithQuote();
        var handler = BuildHandler(soRepo, logger);

        var dto = await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            order.Id, "Approved", null, "ext-1", "provider-x"));

        dto.Status.Should().Be(AppQuoteStatus.CustomerApproved);
        dto.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ExternalRejectionWithReason_ReturnsRejectedQuoteDto()
    {
        var (soRepo, order, logger) = await SetupOrderWithQuote();
        var handler = BuildHandler(soRepo, logger);

        var dto = await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            order.Id, "Rejected", "Preço elevado", "ext-2", "provider-x"));

        dto.Status.Should().Be(AppQuoteStatus.CustomerRejected);
        dto.RejectionReason.Should().Be("Preço elevado");
    }

    [Fact]
    public async Task Handle_ValidDecision_LogsReceivedAndProcessedMessages()
    {
        var (soRepo, order, logger) = await SetupOrderWithQuote();
        var handler = BuildHandler(soRepo, logger);

        await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            order.Id, "Approved", null, "ext-3", "provider-x"));

        logger.Messages.Should().Contain(m => m.Contains("external_quote_decision_received"));
        logger.Messages.Should().Contain(m => m.Contains("external_quote_decision_processed"));
    }

    // ── Validation ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithEmptyServiceOrderId_ThrowsDomainException()
    {
        var handler = BuildHandler(new FakeServiceOrderRepository(), new ListLogger<HandleExternalQuoteDecisionHandler>());

        var act = async () => await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            Guid.Empty, "Approved", null, null, "provider-x"));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage(DomainErrorMessages.ExternalServiceOrderIdRequired);
    }

    [Fact]
    public async Task Handle_WithEmptySource_ThrowsDomainException()
    {
        var handler = BuildHandler(new FakeServiceOrderRepository(), new ListLogger<HandleExternalQuoteDecisionHandler>());

        var act = async () => await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            Guid.NewGuid(), "Approved", null, null, string.Empty));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage(DomainErrorMessages.ExternalSourceRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("approved")]
    [InlineData("Invalid")]
    public async Task Handle_WithInvalidDecision_ThrowsDomainException(string? decision)
    {
        var handler = BuildHandler(new FakeServiceOrderRepository(), new ListLogger<HandleExternalQuoteDecisionHandler>());

        var act = async () => await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            Guid.NewGuid(), decision, null, null, "provider-x"));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage(DomainErrorMessages.ExternalQuoteDecisionInvalid);
    }

    [Fact]
    public async Task Handle_RejectionWithoutReason_ThrowsDomainException()
    {
        var (soRepo, order, logger) = await SetupOrderWithQuote();
        var handler = BuildHandler(soRepo, logger);

        var act = async () => await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            order.Id, "Rejected", string.Empty, null, "provider-x"));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage(DomainErrorMessages.QuoteRejectionReasonRequired);
    }

    [Fact]
    public async Task Handle_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var handler = BuildHandler(new FakeServiceOrderRepository(), new ListLogger<HandleExternalQuoteDecisionHandler>());

        var act = async () => await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            Guid.NewGuid(), "Approved", null, null, "provider-x"));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyDecided_ThrowsExternalQuoteDecisionConflictException()
    {
        var (soRepo, order, logger) = await SetupOrderWithQuote();
        var handler = BuildHandler(soRepo, logger);
        await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            order.Id, "Approved", null, "ext-4", "provider-x"));

        var act = async () => await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            order.Id, "Approved", null, "ext-5", "provider-x"));

        await act.Should().ThrowAsync<ExternalQuoteDecisionConflictException>();
    }

    [Fact]
    public async Task Handle_WithInvalidDecision_LogsReceivedButNotProcessed()
    {
        var logger = new ListLogger<HandleExternalQuoteDecisionHandler>();
        var handler = BuildHandler(new FakeServiceOrderRepository(), logger);

        var act = async () => await handler.HandleAsync(new HandleExternalQuoteDecisionCommand(
            Guid.NewGuid(), "Invalid", null, "ext-invalid", "provider-x"));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage(DomainErrorMessages.ExternalQuoteDecisionInvalid);
        logger.Messages.Should().Contain(m => m.Contains("external_quote_decision_received"));
        logger.Messages.Should().NotContain(m => m.Contains("external_quote_decision_processed"));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static HandleExternalQuoteDecisionHandler BuildHandler(
        FakeServiceOrderRepository soRepo,
        ListLogger<HandleExternalQuoteDecisionHandler> logger) =>
        new(new AcceptQuoteHandler(soRepo), new RejectQuoteHandler(soRepo), logger);

    private static Service CreateServiceWithComposition(
        FakePartRepository partRepo,
        FakeSupplyRepository supplyRepo)
    {
        var part = Part.Create("Filtro de Óleo", ValidPartCode, ValidPartSku, "un", 30m);
        partRepo.AddAsync(part).GetAwaiter().GetResult();

        var supply = Supply.Create("Óleo Motor", ValidSupplyCode, "L", 20m);
        supplyRepo.AddAsync(supply).GetAwaiter().GetResult();

        var service = Service.Create("SVC-EXT-001", "Troca de Óleo", null, 100m, 60);
        service.AddPart(part.Id, "Filtro de Óleo", 1);
        service.AddSupply(supply.Id, "Óleo Motor", 4m, SupplyUnit.Liter);
        return service;
    }

    private static ServiceOrder CreateConsolidatedOrder(Service service, FakeServiceOrderRepository soRepo)
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnostic(Guid.NewGuid());
        order.AddDiagnosticService(service.Id);
        order.CompleteDiagnostic("Diagnóstico concluído.");
        order.ConsolidateDiagnosticServices();
        soRepo.AddAsync(order).GetAwaiter().GetResult();
        return order;
    }

    private async Task<(FakeServiceOrderRepository soRepo, ServiceOrder order, ListLogger<HandleExternalQuoteDecisionHandler> logger)>
        SetupOrderWithQuote()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var service = CreateServiceWithComposition(partRepo, supplyRepo);
        await svcRepo.AddAsync(service);
        var order = CreateConsolidatedOrder(service, soRepo);

        var generateHandler = new GenerateQuoteHandler(soRepo, svcRepo, partRepo, supplyRepo);
        await generateHandler.HandleAsync(new GenerateQuoteCommand(order.Id));

        return (soRepo, order, new ListLogger<HandleExternalQuoteDecisionHandler>());
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));

        private static readonly IDisposable NullScope = new NoopDisposable();

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
