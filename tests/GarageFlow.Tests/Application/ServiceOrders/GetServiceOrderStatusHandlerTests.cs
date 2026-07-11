using FluentAssertions;
using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.Handlers;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Supplies;
using GarageFlow.Tests.Application.Parts;
using GarageFlow.Tests.Application.Services;
using GarageFlow.Tests.Application.Supplies;
using AppServiceOrderStatus = GarageFlow.Application.ServiceOrders.Enums.ServiceOrderStatus;

namespace GarageFlow.Tests.Application.ServiceOrders;

public sealed class GetServiceOrderStatusHandlerTests
{
    [Fact]
    public async Task GetStatus_WithExistingServiceOrder_ReturnsStatusDto()
    {
        var soRepo = new FakeServiceOrderRepository();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await soRepo.AddAsync(order);

        var handler = new GetServiceOrderStatusHandler(soRepo);
        var dto = await handler.HandleAsync(new GetServiceOrderStatusQuery(order.Id));

        dto.ServiceOrderId.Should().Be(order.Id);
        dto.Status.Should().Be(AppServiceOrderStatus.Received);
        dto.Label.Should().Be("Recebida");
        dto.UpdatedAt.Should().Be(order.UpdatedAt);
    }

    [Fact]
    public async Task GetStatus_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var handler = new GetServiceOrderStatusHandler(new FakeServiceOrderRepository());

        var act = async () => await handler.HandleAsync(new GetServiceOrderStatusQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetStatus_AfterQuoteAccepted_ReturnsApproved()
    {
        var (soRepo, order) = await SetupOrderWithQuote();
        await new AcceptQuoteHandler(soRepo).HandleAsync(new AcceptQuoteCommand(order.Id));

        var handler = new GetServiceOrderStatusHandler(soRepo);
        var dto = await handler.HandleAsync(new GetServiceOrderStatusQuery(order.Id));

        dto.Status.Should().Be(AppServiceOrderStatus.Approved);
        dto.Label.Should().Be("Orçamento aprovado");
    }

    [Fact]
    public async Task GetStatus_AfterQuoteRejected_ReturnsRejected()
    {
        var (soRepo, order) = await SetupOrderWithQuote();
        await new RejectQuoteHandler(soRepo).HandleAsync(new RejectQuoteCommand(order.Id, "Valor fora do orçamento"));

        var handler = new GetServiceOrderStatusHandler(soRepo);
        var dto = await handler.HandleAsync(new GetServiceOrderStatusQuery(order.Id));

        dto.Status.Should().Be(AppServiceOrderStatus.Rejected);
        dto.Label.Should().Be("Orçamento recusado");
    }

    private static async Task<(FakeServiceOrderRepository soRepo, ServiceOrder order)> SetupOrderWithQuote()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var part = Part.Create("Filtro de Óleo", "PRT-STA-001", "SKU-STA-001", "un", 30m);
        await partRepo.AddAsync(part);

        var supply = Supply.Create("Óleo Motor", "INS-STA-001", "L", 20m);
        await supplyRepo.AddAsync(supply);

        var service = Service.Create("SVC-STA-001", "Troca de Óleo", null, 100m, 60);
        service.AddPart(part.Id, "Filtro de Óleo", 1);
        service.AddSupply(supply.Id, "Óleo Motor", 4m, SupplyUnit.Liter);
        await svcRepo.AddAsync(service);

        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnostic(Guid.NewGuid());
        order.AddDiagnosticService(service.Id);
        order.CompleteDiagnostic("Diagnóstico concluído.");
        order.ConsolidateDiagnosticServices();
        await soRepo.AddAsync(order);

        var generateHandler = new GenerateQuoteHandler(soRepo, svcRepo, partRepo, supplyRepo);
        await generateHandler.HandleAsync(new GenerateQuoteCommand(order.Id));

        return (soRepo, order);
    }
}
