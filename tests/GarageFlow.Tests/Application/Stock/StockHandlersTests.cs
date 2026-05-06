using FluentAssertions;
using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.Handlers;
using GarageFlow.Application.Stock.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;
using GarageFlow.Tests.Application.Parts;
using GarageFlow.Tests.Application.Supplies;

namespace GarageFlow.Tests.Application.Stock;

public sealed class StockHandlersTests
{
    [Fact]
    public async Task CreateStockEntry_WhenStockDoesNotExist_ShouldCreateAndAddQuantity()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var part = Part.Create("Filtro de óleo", "P-ST-001", "SKU-ST-001", "UN", 20m);
        await partRepo.AddAsync(part);

        var handler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        var result = await handler.HandleAsync(new CreateStockEntryCommand(part.Id, StockItemType.Part, 10m, 2m, null, null));

        result.TotalQuantity.Should().Be(10m);
        result.AvailableQuantity.Should().Be(10m);
    }

    [Fact]
    public async Task ReserveStock_WhenInsufficientAvailable_ShouldThrowConflict()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var part = Part.Create("Pastilha", "P-ST-002", "SKU-ST-002", "UN", 40m);
        await partRepo.AddAsync(part);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, StockItemType.Part, 5m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        var act = async () => await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, StockItemType.Part, 8m, null, null));

        await act.Should().ThrowAsync<StockQuantityConflictException>();
    }

    [Fact]
    public async Task ReleaseStock_ForSupply_WithReason_ShouldReleaseReservedQuantity()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var supply = Supply.Create("Óleo 5W30", "S-ST-001", "L", 30m, null);
        await supplyRepo.AddAsync(supply);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(supply.Id, StockItemType.Supply, 20m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(supply.Id, StockItemType.Supply, 5m, null, null));

        var releaseHandler = new ReleaseStockReservationHandler(stockRepo, partRepo, supplyRepo);
        var result = await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(supply.Id, StockItemType.Supply, 1m, "Ajuste manual", "operador.teste", null, null));

        result.ReservedQuantity.Should().Be(4m);
        result.AvailableQuantity.Should().Be(16m);
    }

    [Fact]
    public async Task ReleaseStock_WithoutReason_ShouldThrowDomainException()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var part = Part.Create("Pastilha", "P-ST-004", "SKU-ST-004", "UN", 40m);
        await partRepo.AddAsync(part);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, StockItemType.Part, 5m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, StockItemType.Part, 2m, null, null));

        var releaseHandler = new ReleaseStockReservationHandler(stockRepo, partRepo, supplyRepo);
        var act = async () => await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(part.Id, StockItemType.Part, 1m, null, "operador.teste", null, null));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task ReleaseStock_WithoutPerformedBy_ShouldThrowDomainException()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var part = Part.Create("Pastilha", "P-ST-005", "SKU-ST-005", "UN", 40m);
        await partRepo.AddAsync(part);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, StockItemType.Part, 5m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, StockItemType.Part, 2m, null, null));

        var releaseHandler = new ReleaseStockReservationHandler(stockRepo, partRepo, supplyRepo);
        var act = async () => await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(part.Id, StockItemType.Part, 1m, "Ajuste manual", null, null, null));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task ReleaseStock_WithAuditTrail_ShouldPersistAllAuditFields()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var part = Part.Create("Correia B", "P-ST-006", "SKU-ST-006", "UN", 50m);
        await partRepo.AddAsync(part);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, StockItemType.Part, 10m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, StockItemType.Part, 3m, null, null));

        var referenceId = Guid.NewGuid();
        var releaseHandler = new ReleaseStockReservationHandler(stockRepo, partRepo, supplyRepo);
        await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(
            part.Id, StockItemType.Part, 2m, "Cancelamento manual", "gestor.estoque", referenceId, "SeparationOrder"));

        var stock = await stockRepo.GetByItemAsync(part.Id, StockItemType.Part);
        var releaseOp = stock!.Operations.Last();

        releaseOp.Type.Should().Be(StockOperationType.Release);
        releaseOp.PerformedBy.Should().Be("gestor.estoque");
        releaseOp.Reason.Should().Be("Cancelamento manual");
        releaseOp.ReferenceId.Should().Be(referenceId);
        releaseOp.ReferenceType.Should().Be("SeparationOrder");
        releaseOp.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ListStockOperations_ShouldReturnPagedOperations()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var part = Part.Create("Correia", "P-ST-003", "SKU-ST-003", "UN", 35m);
        await partRepo.AddAsync(part);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, StockItemType.Part, 15m, 0m, "Entrada inicial", null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, StockItemType.Part, 3m, "Reserva", null));

        var listHandler = new ListStockOperationsHandler(stockRepo);
        var result = await listHandler.HandleAsync(new ListStockOperationsQuery(part.Id, StockItemType.Part, null, null, 1, 20));

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }
}
