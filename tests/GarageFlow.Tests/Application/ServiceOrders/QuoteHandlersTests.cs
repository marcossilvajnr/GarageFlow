using FluentAssertions;
using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.Handlers;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;
using GarageFlow.Tests.Application.Parts;
using GarageFlow.Tests.Application.Services;
using GarageFlow.Tests.Application.ServiceOrders;
using GarageFlow.Tests.Application.Supplies;

namespace GarageFlow.Tests.Application.ServiceOrders;

public sealed class QuoteHandlersTests
{
    private static readonly string ValidPartCode = "PRT-Q-001";
    private static readonly string ValidPartSku = "SKU-Q-001";
    private static readonly string ValidSupplyCode = "INS-Q-001";

    private static Service CreateServiceWithComposition(
        FakePartRepository partRepo,
        FakeSupplyRepository supplyRepo,
        out Part part,
        out Supply supply)
    {
        part = Part.Create("Filtro de Óleo", ValidPartCode, ValidPartSku, "un", 30m);
        var partId = part.Id;
        partRepo.AddAsync(part).GetAwaiter().GetResult();

        supply = Supply.Create("Óleo Motor", ValidSupplyCode, "L", 20m);
        var supplyId = supply.Id;
        supplyRepo.AddAsync(supply).GetAwaiter().GetResult();

        var service = Service.Create("SVC-Q-001", "Troca de Óleo", null, 100m, 60);
        service.AddPart(partId, "Filtro de Óleo", 1);
        service.AddSupply(supplyId, "Óleo Motor", 4m, global::GarageFlow.Domain.Supplies.SupplyUnit.Liter);
        return service;
    }

    private static ServiceOrder CreateConsolidatedOrder(Service service, FakeServiceOrderRepository soRepo)
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnostic(Guid.NewGuid());
        order.AddDiagnosticService(service.Id);
        order.CompleteDiagnostic("Diagnóstico concluído.");
        order.ConsolidateDiagnosticServices();
        soRepo.AddAsync(order).GetAwaiter().GetResult();
        return order;
    }

    // ── GenerateQuoteHandler ────────────────────────────────────────────────

    [Fact]
    public async Task GenerateQuote_WithConsolidatedServices_ReturnsQuoteDto()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var service = CreateServiceWithComposition(partRepo, supplyRepo, out var part, out var supply);
        await svcRepo.AddAsync(service);

        var order = CreateConsolidatedOrder(service, soRepo);

        var handler = new GenerateQuoteHandler(soRepo, svcRepo, partRepo, supplyRepo);
        var dto = await handler.HandleAsync(new GenerateQuoteCommand(order.Id));

        dto.Should().NotBeNull();
        dto.Status.Should().Be(QuoteStatus.WaitingCustomerApproval);
        dto.Items.Should().HaveCount(1);
        dto.Items[0].LaborPrice.Should().Be(100m);
        dto.Items[0].PartsTotal.Should().Be(30m);   // 30 * 1
        dto.Items[0].SuppliesTotal.Should().Be(80m); // 20 * 4
        dto.Items[0].Subtotal.Should().Be(210m);    // 100 + 30 + 80
        dto.TotalAmount.Should().Be(210m);
    }

    [Fact]
    public async Task GenerateQuote_WithNoActiveServices_ThrowsNoConsolidatedServicesException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await soRepo.AddAsync(order);

        var handler = new GenerateQuoteHandler(
            soRepo,
            new FakeServiceRepository(),
            new FakePartRepository(),
            new FakeSupplyRepository());

        var act = async () => await handler.HandleAsync(new GenerateQuoteCommand(order.Id));

        await act.Should().ThrowAsync<NoConsolidatedServicesException>();
    }

    [Fact]
    public async Task GenerateQuote_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var handler = new GenerateQuoteHandler(
            new FakeServiceOrderRepository(),
            new FakeServiceRepository(),
            new FakePartRepository(),
            new FakeSupplyRepository());

        var act = async () => await handler.HandleAsync(new GenerateQuoteCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GenerateQuote_WithInactiveService_ThrowsServiceNotAvailableForQuoteException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();

        var service = Service.Create("SVC-Q-002", "Serviço Inativo", null, 50m, 30);
        await svcRepo.AddAsync(service);
        service.Deactivate();

        var order = CreateConsolidatedOrder(service, soRepo);

        var handler = new GenerateQuoteHandler(soRepo, svcRepo, new FakePartRepository(), new FakeSupplyRepository());

        var act = async () => await handler.HandleAsync(new GenerateQuoteCommand(order.Id));

        await act.Should().ThrowAsync<ServiceNotAvailableForQuoteException>();
    }

    [Fact]
    public async Task GenerateQuote_WhenQuoteAlreadyExists_ThrowsQuoteAlreadyExistsException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var service = CreateServiceWithComposition(partRepo, supplyRepo, out _, out _);
        await svcRepo.AddAsync(service);

        var order = CreateConsolidatedOrder(service, soRepo);

        var handler = new GenerateQuoteHandler(soRepo, svcRepo, partRepo, supplyRepo);
        await handler.HandleAsync(new GenerateQuoteCommand(order.Id));

        var act = async () => await handler.HandleAsync(new GenerateQuoteCommand(order.Id));

        await act.Should().ThrowAsync<QuoteAlreadyExistsException>();
    }

    // ── GetServiceOrderQuoteHandler ─────────────────────────────────────────

    [Fact]
    public async Task GetQuote_WithExistingQuote_ReturnsQuoteDto()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var service = CreateServiceWithComposition(partRepo, supplyRepo, out _, out _);
        await svcRepo.AddAsync(service);
        var order = CreateConsolidatedOrder(service, soRepo);

        var generateHandler = new GenerateQuoteHandler(soRepo, svcRepo, partRepo, supplyRepo);
        await generateHandler.HandleAsync(new GenerateQuoteCommand(order.Id));

        var getHandler = new GetServiceOrderQuoteHandler(soRepo);
        var dto = await getHandler.HandleAsync(new GetServiceOrderQuoteQuery(order.Id));

        dto.Should().NotBeNull();
        dto.Status.Should().Be(QuoteStatus.WaitingCustomerApproval);
    }

    [Fact]
    public async Task GetQuote_WithNoQuote_ThrowsQuoteNotFoundException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await soRepo.AddAsync(order);

        var handler = new GetServiceOrderQuoteHandler(soRepo);

        var act = async () => await handler.HandleAsync(new GetServiceOrderQuoteQuery(order.Id));

        await act.Should().ThrowAsync<QuoteNotFoundException>();
    }

    [Fact]
    public async Task GetQuote_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var handler = new GetServiceOrderQuoteHandler(new FakeServiceOrderRepository());

        var act = async () => await handler.HandleAsync(new GetServiceOrderQuoteQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── AcceptQuoteHandler ──────────────────────────────────────────────────

    [Fact]
    public async Task AcceptQuote_WhenPending_ReturnsAcceptedQuoteDto()
    {
        var (soRepo, svcRepo, partRepo, supplyRepo, order) = await SetupOrderWithQuote();

        var handler = new AcceptQuoteHandler(soRepo);
        var dto = await handler.HandleAsync(new AcceptQuoteCommand(order.Id));

        dto.Status.Should().Be(QuoteStatus.CustomerApproved);
        dto.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task AcceptQuote_WhenPending_SetsServiceOrderStatusToQuoteApproved()
    {
        var (soRepo, _, _, _, order) = await SetupOrderWithQuote();

        await new AcceptQuoteHandler(soRepo).HandleAsync(new AcceptQuoteCommand(order.Id));

        order.Status.Should().Be(ServiceOrderStatus.Approved);
    }

    [Fact]
    public async Task AcceptQuote_WhenAlreadyDecided_ThrowsQuoteAlreadyDecidedException()
    {
        var (soRepo, _, _, _, order) = await SetupOrderWithQuote();
        await new AcceptQuoteHandler(soRepo).HandleAsync(new AcceptQuoteCommand(order.Id));

        var act = async () =>
            await new AcceptQuoteHandler(soRepo).HandleAsync(new AcceptQuoteCommand(order.Id));

        await act.Should().ThrowAsync<QuoteAlreadyDecidedException>();
    }

    [Fact]
    public async Task AcceptQuote_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var handler = new AcceptQuoteHandler(new FakeServiceOrderRepository());

        var act = async () => await handler.HandleAsync(new AcceptQuoteCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AcceptQuote_WithNoQuote_ThrowsQuoteNotFoundException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await soRepo.AddAsync(order);

        var act = async () =>
            await new AcceptQuoteHandler(soRepo).HandleAsync(new AcceptQuoteCommand(order.Id));

        await act.Should().ThrowAsync<QuoteNotFoundException>();
    }

    // ── RejectQuoteHandler ──────────────────────────────────────────────────

    [Fact]
    public async Task RejectQuote_WhenPending_ReturnsRejectedQuoteDto()
    {
        var (soRepo, _, _, _, order) = await SetupOrderWithQuote();

        var handler = new RejectQuoteHandler(soRepo);
        var dto = await handler.HandleAsync(new RejectQuoteCommand(order.Id, "Preço elevado"));

        dto.Status.Should().Be(QuoteStatus.CustomerRejected);
        dto.RejectedAt.Should().NotBeNull();
        dto.RejectionReason.Should().Be("Preço elevado");
    }

    [Fact]
    public async Task RejectQuote_WhenPending_SetsServiceOrderStatusToQuoteRejected()
    {
        var (soRepo, _, _, _, order) = await SetupOrderWithQuote();

        await new RejectQuoteHandler(soRepo).HandleAsync(new RejectQuoteCommand(order.Id, "Valor fora do orçamento"));

        order.Status.Should().Be(ServiceOrderStatus.Rejected);
    }

    [Fact]
    public async Task RejectQuote_WhenAlreadyDecided_ThrowsQuoteAlreadyDecidedException()
    {
        var (soRepo, _, _, _, order) = await SetupOrderWithQuote();
        await new RejectQuoteHandler(soRepo).HandleAsync(new RejectQuoteCommand(order.Id, "Primeira rejeição"));

        var act = async () =>
            await new RejectQuoteHandler(soRepo).HandleAsync(new RejectQuoteCommand(order.Id, "Segunda rejeição"));

        await act.Should().ThrowAsync<QuoteAlreadyDecidedException>();
    }

    [Fact]
    public async Task RejectQuote_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var handler = new RejectQuoteHandler(new FakeServiceOrderRepository());

        var act = async () =>
            await handler.HandleAsync(new RejectQuoteCommand(Guid.NewGuid(), "Motivo"));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task RejectQuote_WithNoQuote_ThrowsQuoteNotFoundException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await soRepo.AddAsync(order);

        var act = async () =>
            await new RejectQuoteHandler(soRepo).HandleAsync(new RejectQuoteCommand(order.Id, "Motivo"));

        await act.Should().ThrowAsync<QuoteNotFoundException>();
    }

    [Fact]
    public async Task RejectQuote_WithEmptyReason_ThrowsDomainException()
    {
        var (soRepo, _, _, _, order) = await SetupOrderWithQuote();

        var act = async () =>
            await new RejectQuoteHandler(soRepo).HandleAsync(new RejectQuoteCommand(order.Id, string.Empty));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage(DomainErrorMessages.QuoteRejectionReasonRequired);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task<(FakeServiceOrderRepository soRepo, FakeServiceRepository svcRepo,
        FakePartRepository partRepo, FakeSupplyRepository supplyRepo, ServiceOrder order)>
        SetupOrderWithQuote()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var service = CreateServiceWithComposition(partRepo, supplyRepo, out _, out _);
        await svcRepo.AddAsync(service);
        var order = CreateConsolidatedOrder(service, soRepo);

        var generateHandler = new GenerateQuoteHandler(soRepo, svcRepo, partRepo, supplyRepo);
        await generateHandler.HandleAsync(new GenerateQuoteCommand(order.Id));

        return (soRepo, svcRepo, partRepo, supplyRepo, order);
    }
}
