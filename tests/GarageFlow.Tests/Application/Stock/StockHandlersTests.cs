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
using AppStockItemType = GarageFlow.Application.Stock.Enums.StockItemType;

namespace GarageFlow.Tests.Application.Stock;

public sealed class StockHandlersTests
{
    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static async Task<(FakeStockRepository, FakeSeparationOrderRepository, Part)>
        SetupStockWithCompletedSeparationAsync(string code, decimal initialQty, int separatedQty)
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        var separationRepo = new FakeSeparationOrderRepository();

        var part = Part.Create($"Peça {code}", $"P-{code}", $"SKU-{code}", "UN", 40m);
        await partRepo.AddAsync(part);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, AppStockItemType.Part, initialQty, 0m, null, null));

        // Create a separation order that goes all the way to Completed
        var partItem = SeparationPartItem.Create(part.Id, part.Name, separatedQty);
        var separationOrder = SeparationOrder.Create(Guid.NewGuid(), [partItem], []);
        separationOrder.Reserve();
        separationOrder.ConfirmStockistWithdrawal(Guid.NewGuid());
        separationOrder.ConfirmMechanicReceipt();
        await separationRepo.AddAsync(separationOrder);

        return (stockRepo, separationRepo, part);
    }

    private static ReleaseStockReservationHandler BuildReleaseHandler(
        FakeStockRepository stockRepo,
        FakePartRepository partRepo,
        FakeSupplyRepository supplyRepo,
        FakeSeparationOrderRepository separationRepo)
        => new(stockRepo, partRepo, supplyRepo, separationRepo);

    // ---------------------------------------------------------------------------
    // Existing tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CreateStockEntry_WhenStockDoesNotExist_ShouldCreateAndAddQuantity()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();

        var part = Part.Create("Filtro de óleo", "P-ST-001", "SKU-ST-001", "UN", 20m);
        await partRepo.AddAsync(part);

        var handler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        var result = await handler.HandleAsync(new CreateStockEntryCommand(part.Id, AppStockItemType.Part, 10m, 2m, null, null));

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
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, AppStockItemType.Part, 5m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        var act = async () => await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, AppStockItemType.Part, 8m, null, null));

        await act.Should().ThrowAsync<StockQuantityConflictException>();
    }

    [Fact]
    public async Task ReleaseStock_ForSupply_WithReason_ShouldReleaseReservedQuantity()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        var separationRepo = new FakeSeparationOrderRepository();

        var supply = Supply.Create("Óleo 5W30", "S-ST-001", "L", 30m, null);
        await supplyRepo.AddAsync(supply);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(supply.Id, AppStockItemType.Supply, 20m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(supply.Id, AppStockItemType.Supply, 5m, null, null));

        var releaseHandler = BuildReleaseHandler(stockRepo, partRepo, supplyRepo, separationRepo);
        var result = await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(supply.Id, AppStockItemType.Supply, 1m, "Ajuste manual", "operador.teste", null, null));

        result.ReservedQuantity.Should().Be(4m);
        result.AvailableQuantity.Should().Be(16m);
    }

    [Fact]
    public async Task ReleaseStock_WithoutReason_ShouldThrowDomainException()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        var separationRepo = new FakeSeparationOrderRepository();

        var part = Part.Create("Pastilha", "P-ST-004", "SKU-ST-004", "UN", 40m);
        await partRepo.AddAsync(part);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, AppStockItemType.Part, 5m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, AppStockItemType.Part, 2m, null, null));

        var releaseHandler = BuildReleaseHandler(stockRepo, partRepo, supplyRepo, separationRepo);
        var act = async () => await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(part.Id, AppStockItemType.Part, 1m, null, "operador.teste", null, null));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task ReleaseStock_WithoutPerformedBy_ShouldThrowDomainException()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        var separationRepo = new FakeSeparationOrderRepository();

        var part = Part.Create("Pastilha", "P-ST-005", "SKU-ST-005", "UN", 40m);
        await partRepo.AddAsync(part);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, AppStockItemType.Part, 5m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, AppStockItemType.Part, 2m, null, null));

        var releaseHandler = BuildReleaseHandler(stockRepo, partRepo, supplyRepo, separationRepo);
        var act = async () => await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(part.Id, AppStockItemType.Part, 1m, "Ajuste manual", null, null, null));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task ReleaseStock_WithAuditTrail_ShouldPersistAllAuditFields()
    {
        var stockRepo = new FakeStockRepository();
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        var separationRepo = new FakeSeparationOrderRepository();

        var part = Part.Create("Correia B", "P-ST-006", "SKU-ST-006", "UN", 50m);
        await partRepo.AddAsync(part);

        var createHandler = new CreateStockEntryHandler(stockRepo, partRepo, supplyRepo);
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, AppStockItemType.Part, 10m, 0m, null, null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, AppStockItemType.Part, 3m, null, null));

        var referenceId = Guid.NewGuid();
        var releaseHandler = BuildReleaseHandler(stockRepo, partRepo, supplyRepo, separationRepo);
        await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(
            part.Id, AppStockItemType.Part, 2m, "Cancelamento manual", "gestor.estoque", referenceId, "SeparationOrder"));

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
        await createHandler.HandleAsync(new CreateStockEntryCommand(part.Id, AppStockItemType.Part, 15m, 0m, "Entrada inicial", null));

        var reserveHandler = new ReserveStockHandler(stockRepo, partRepo, supplyRepo);
        await reserveHandler.HandleAsync(new ReserveStockCommand(part.Id, AppStockItemType.Part, 3m, "Reserva", null));

        var listHandler = new ListStockOperationsHandler(stockRepo);
        var result = await listHandler.HandleAsync(new ListStockOperationsQuery(part.Id, AppStockItemType.Part, null, null, 1, 20));

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    // ---------------------------------------------------------------------------
    // Task-033: post-custody exceptional adjustment tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ReleaseStock_PostCustody_WithoutReferenceId_ShouldThrowDomainException()
    {
        var (stockRepo, separationRepo, part) = await SetupStockWithCompletedSeparationAsync("T33A", 10m, 3);
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        await partRepo.AddAsync(part);

        // Manually add reserved quantity so there's something to release
        var stock = await stockRepo.GetByItemAsync(part.Id, StockItemType.Part);
        stock!.Reserve(2m);

        var releaseHandler = BuildReleaseHandler(stockRepo, partRepo, supplyRepo, separationRepo);
        var act = async () => await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(
            part.Id, AppStockItemType.Part, 1m, "Ajuste excepcional", "admin.user", null, null));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*obrigatória*");
    }

    [Fact]
    public async Task ReleaseStock_PostCustody_WithoutReferenceType_ShouldThrowDomainException()
    {
        var (stockRepo, separationRepo, part) = await SetupStockWithCompletedSeparationAsync("T33B", 10m, 3);
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        await partRepo.AddAsync(part);

        var stock = await stockRepo.GetByItemAsync(part.Id, StockItemType.Part);
        stock!.Reserve(2m);

        var releaseHandler = BuildReleaseHandler(stockRepo, partRepo, supplyRepo, separationRepo);
        var act = async () => await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(
            part.Id, AppStockItemType.Part, 1m, "Ajuste excepcional", "admin.user", Guid.NewGuid(), null));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*obrigatório*");
    }

    [Fact]
    public async Task ReleaseStock_PostCustody_WithValidReference_ShouldSucceedAndPreserveInvariant()
    {
        var (stockRepo, separationRepo, part) = await SetupStockWithCompletedSeparationAsync("T33C", 10m, 3);
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        await partRepo.AddAsync(part);

        // Add reserved quantity to release
        var stock = await stockRepo.GetByItemAsync(part.Id, StockItemType.Part);
        stock!.Reserve(4m);

        var completedOrder = await separationRepo.GetByIdAsync(
            (await separationRepo.ListAsync(1, 10)).Items.First().Id);

        var releaseHandler = BuildReleaseHandler(stockRepo, partRepo, supplyRepo, separationRepo);
        var result = await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(
            part.Id, AppStockItemType.Part, 2m, "Ajuste excepcional pós-custódia", "admin.user",
            completedOrder!.Id, "SeparationOrder"));

        result.ReservedQuantity.Should().Be(2m);
        result.AvailableQuantity.Should().BeGreaterThanOrEqualTo(0m);
        result.TotalQuantity.Should().BeGreaterThanOrEqualTo(result.ReservedQuantity);
    }

    [Fact]
    public async Task ReleaseStock_PostCustody_WithInvalidReferenceId_ShouldThrowEntityNotFoundException()
    {
        var (stockRepo, separationRepo, part) = await SetupStockWithCompletedSeparationAsync("T33D", 10m, 3);
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        await partRepo.AddAsync(part);

        var stock = await stockRepo.GetByItemAsync(part.Id, StockItemType.Part);
        stock!.Reserve(2m);

        var releaseHandler = BuildReleaseHandler(stockRepo, partRepo, supplyRepo, separationRepo);
        var act = async () => await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(
            part.Id, AppStockItemType.Part, 1m, "Ajuste excepcional", "admin.user",
            Guid.NewGuid(), "SeparationOrder")); // non-existent referenceId

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ReleaseStock_PostCustody_WithUnsupportedReferenceType_ShouldThrowDomainException()
    {
        var (stockRepo, separationRepo, part) = await SetupStockWithCompletedSeparationAsync("T33E", 10m, 3);
        var partRepo = new FakePartRepository();
        var supplyRepo = new FakeSupplyRepository();
        await partRepo.AddAsync(part);

        var stock = await stockRepo.GetByItemAsync(part.Id, StockItemType.Part);
        stock!.Reserve(2m);

        var completedOrder = await separationRepo.GetByIdAsync(
            (await separationRepo.ListAsync(1, 10)).Items.First().Id);

        var releaseHandler = BuildReleaseHandler(stockRepo, partRepo, supplyRepo, separationRepo);
        var act = async () => await releaseHandler.HandleAsync(new ReleaseStockReservationCommand(
            part.Id, AppStockItemType.Part, 1m, "Ajuste excepcional", "admin.user",
            completedOrder!.Id, "ServiceOrder"));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*referência*");
    }
}
