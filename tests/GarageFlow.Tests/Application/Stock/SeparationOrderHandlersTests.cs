using FluentAssertions;
using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.Handlers;
using GarageFlow.Application.Stock.Queries;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;
using GarageFlow.Tests.Application.Executions;
using DomainStock = GarageFlow.Domain.Stock.Stock;

namespace GarageFlow.Tests.Application.Stock;

public sealed class SeparationOrderHandlersTests
{
    private static async Task<(CreateSeparationOrderCommand Command, FakeStockRepository StockRepo)> BuildCommandWithStockAsync(
        Guid? executionOrderId = null,
        decimal stockQuantity = 100m)
    {
        var partId = Guid.NewGuid();
        var command = new CreateSeparationOrderCommand(
            executionOrderId ?? Guid.NewGuid(),
            [new CreateSeparationPartItemCommand(partId, "Filtro de óleo", 2)],
            []);

        var stockRepo = new FakeStockRepository();
        var stock = DomainStock.Create(partId, StockItemType.Part, stockQuantity, 0);
        await stockRepo.AddAsync(stock);

        return (command, stockRepo);
    }

    private static CreateSeparationOrderCommand ValidCreateCommand(Guid? executionOrderId = null) =>
        new(
            executionOrderId ?? Guid.NewGuid(),
            [new CreateSeparationPartItemCommand(Guid.NewGuid(), "Filtro de óleo", 2)],
            []);

    private static async Task<ExecutionOrder> AddPendingExecutionOrder(FakeExecutionOrderRepository repository)
    {
        var executionOrder = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await repository.AddAsync(executionOrder);
        return executionOrder;
    }

    // --- CreateSeparationOrderHandler ---

    [Fact]
    public async Task CreateSeparationOrder_WithValidData_ReturnsDtoWithStatusPending()
    {
        var repo = new FakeSeparationOrderRepository();
        var handler = new CreateSeparationOrderHandler(repo);
        var command = ValidCreateCommand();

        var dto = await handler.HandleAsync(command);

        dto.Should().NotBeNull();
        dto.Id.Should().NotBeEmpty();
        dto.ExecutionOrderId.Should().Be(command.ExecutionOrderId);
        dto.Status.Should().Be(SeparationOrderStatus.Pending);
        dto.Parts.Should().HaveCount(1);
        dto.Supplies.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateSeparationOrder_WithEmptyExecutionOrderId_ThrowsDomainException()
    {
        var repo = new FakeSeparationOrderRepository();
        var handler = new CreateSeparationOrderHandler(repo);
        var command = ValidCreateCommand(Guid.Empty);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Ordem de Execução é obrigatória");
    }

    [Fact]
    public async Task CreateSeparationOrder_WithNoItems_ThrowsDomainException()
    {
        var repo = new FakeSeparationOrderRepository();
        var handler = new CreateSeparationOrderHandler(repo);
        var command = new CreateSeparationOrderCommand(Guid.NewGuid(), [], []);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Separação deve ter pelo menos um item");
    }

    // --- GetSeparationOrderByIdHandler ---

    [Fact]
    public async Task GetSeparationOrderById_WhenExists_ReturnsDto()
    {
        var repo = new FakeSeparationOrderRepository();
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var getHandler = new GetSeparationOrderByIdHandler(repo);
        var result = await getHandler.HandleAsync(new GetSeparationOrderByIdQuery(dto.Id));

        result.Should().NotBeNull();
        result.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task GetSeparationOrderById_WhenNotExists_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSeparationOrderRepository();
        var handler = new GetSeparationOrderByIdHandler(repo);

        var act = async () => await handler.HandleAsync(new GetSeparationOrderByIdQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- ReserveSeparationOrderHandler ---

    [Fact]
    public async Task Reserve_WhenPending_ReturnsWaitingPickupStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);

        var result = await new ReserveSeparationOrderHandler(repo, stockRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        result.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    [Fact]
    public async Task Reserve_WhenPending_ReducesStockAvailability()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync(stockQuantity: 50m);
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);

        await new ReserveSeparationOrderHandler(repo, stockRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var partId = command.Parts[0].PartId;
        var stock = await stockRepo.GetByItemAsync(partId, StockItemType.Part);
        stock!.ReservedQuantity.Should().Be(2m);
        stock.AvailableQuantity.Should().Be(48m);
    }

    [Fact]
    public async Task Reserve_WhenNotPending_ThrowsInvalidSeparationOrderStatusTransitionException()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        var reserveHandler = new ReserveSeparationOrderHandler(repo, stockRepo);
        await reserveHandler.HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var act = async () => await reserveHandler.HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>();
    }

    [Fact]
    public async Task Reserve_WhenNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var handler = new ReserveSeparationOrderHandler(repo, stockRepo);

        var act = async () => await handler.HandleAsync(new ReserveSeparationOrderCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Reserve_WhenPartStockNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository(); // empty — no stock registered
        var command = ValidCreateCommand();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);

        var act = async () => await new ReserveSeparationOrderHandler(repo, stockRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Reserve_WithInsufficientStock_ThrowsStockQuantityConflictException()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync(stockQuantity: 1m); // needs 2, only 1 available
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);

        var act = async () => await new ReserveSeparationOrderHandler(repo, stockRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        await act.Should().ThrowAsync<StockQuantityConflictException>();
    }

    [Fact]
    public async Task Reserve_WithInsufficientStock_DoesNotChangeSeparationStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync(stockQuantity: 1m);
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);

        try { await new ReserveSeparationOrderHandler(repo, stockRepo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id)); }
        catch (StockQuantityConflictException) { }

        var separation = await repo.GetByIdAsync(dto.Id);
        separation!.Status.Should().Be(SeparationOrderStatus.Pending);
    }

    // --- WaitSeparationOrderPurchaseHandler ---

    [Fact]
    public async Task WaitPurchase_WhenPending_ReturnsWaitingPurchaseStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var waitHandler = new WaitSeparationOrderPurchaseHandler(repo);
        var result = await waitHandler.HandleAsync(new WaitSeparationOrderPurchaseCommand(dto.Id));

        result.Status.Should().Be(SeparationOrderStatus.WaitingPurchase);
    }

    [Fact]
    public async Task WaitPurchase_WhenNotPending_ThrowsInvalidSeparationOrderStatusTransitionException()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new ReserveSeparationOrderHandler(repo, stockRepo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var act = async () => await new WaitSeparationOrderPurchaseHandler(repo)
            .HandleAsync(new WaitSeparationOrderPurchaseCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>();
    }

    // --- ResumeSeparationOrderAfterPurchaseHandler ---

    [Fact]
    public async Task ResumeAfterPurchase_WhenWaitingPurchase_ReturnsWaitingPickupStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new WaitSeparationOrderPurchaseHandler(repo).HandleAsync(new WaitSeparationOrderPurchaseCommand(dto.Id));

        var result = await new ResumeSeparationOrderAfterPurchaseHandler(repo, stockRepo)
            .HandleAsync(new ResumeSeparationOrderAfterPurchaseCommand(dto.Id));

        result.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    [Fact]
    public async Task ResumeAfterPurchase_WhenWaitingPurchase_ReservesStockItems()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync(stockQuantity: 50m);
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new WaitSeparationOrderPurchaseHandler(repo).HandleAsync(new WaitSeparationOrderPurchaseCommand(dto.Id));

        await new ResumeSeparationOrderAfterPurchaseHandler(repo, stockRepo)
            .HandleAsync(new ResumeSeparationOrderAfterPurchaseCommand(dto.Id));

        var partId = command.Parts[0].PartId;
        var stock = await stockRepo.GetByItemAsync(partId, StockItemType.Part);
        stock!.ReservedQuantity.Should().Be(2m);
        stock.AvailableQuantity.Should().Be(48m);
    }

    [Fact]
    public async Task ResumeAfterPurchase_WhenNotWaitingPurchase_ThrowsInvalidSeparationOrderStatusTransitionException()
    {
        var repo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(ValidCreateCommand());

        var act = async () => await new ResumeSeparationOrderAfterPurchaseHandler(repo, stockRepo)
            .HandleAsync(new ResumeSeparationOrderAfterPurchaseCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>();
    }

    [Fact]
    public async Task ResumeAfterPurchase_WhenPartStockNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository(); // empty — no stock registered
        var command = ValidCreateCommand();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new WaitSeparationOrderPurchaseHandler(repo).HandleAsync(new WaitSeparationOrderPurchaseCommand(dto.Id));

        var act = async () => await new ResumeSeparationOrderAfterPurchaseHandler(repo, stockRepo)
            .HandleAsync(new ResumeSeparationOrderAfterPurchaseCommand(dto.Id));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ResumeAfterPurchase_WithInsufficientStock_ThrowsStockQuantityConflictException()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync(stockQuantity: 1m); // needs 2
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new WaitSeparationOrderPurchaseHandler(repo).HandleAsync(new WaitSeparationOrderPurchaseCommand(dto.Id));

        var act = async () => await new ResumeSeparationOrderAfterPurchaseHandler(repo, stockRepo)
            .HandleAsync(new ResumeSeparationOrderAfterPurchaseCommand(dto.Id));

        await act.Should().ThrowAsync<StockQuantityConflictException>();
    }

    // --- ConfirmSeparationStockistWithdrawalHandler ---

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenWaitingPickup_ReturnsSeparatedStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new ReserveSeparationOrderHandler(repo, stockRepo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var result = await new ConfirmSeparationStockistWithdrawalHandler(repo, stockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        result.Status.Should().Be(SeparationOrderStatus.Separated);
        result.StockistId.Should().NotBeNull();
        result.ConfirmedByStockistAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenWaitingPickup_ConsumesStockReservation()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync(stockQuantity: 50m);
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new ReserveSeparationOrderHandler(repo, stockRepo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        await new ConfirmSeparationStockistWithdrawalHandler(repo, stockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        var partId = command.Parts[0].PartId;
        var stock = await stockRepo.GetByItemAsync(partId, StockItemType.Part);
        stock!.ReservedQuantity.Should().Be(0m);
        stock.TotalQuantity.Should().Be(48m);
        stock.AvailableQuantity.Should().Be(48m);
    }

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenPartStockNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, _) = await BuildCommandWithStockAsync();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        var emptyStockRepo = new FakeStockRepository();
        // Reserve directly in domain without stock ops to reach WaitingPickup with empty stock repo
        var separation = await repo.GetByIdAsync(dto.Id);
        separation!.Reserve();

        var act = async () => await new ConfirmSeparationStockistWithdrawalHandler(repo, emptyStockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenInsufficientReservedStock_ThrowsStockQuantityConflictException()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync(stockQuantity: 10m);
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new ReserveSeparationOrderHandler(repo, stockRepo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        // Simulate operational adjustment that reduces reserved qty below separation requirement
        var partId = command.Parts[0].PartId;
        var stock = await stockRepo.GetByItemAsync(partId, StockItemType.Part);
        stock!.Release(1m); // ReservedQty drops to 1, separation needs 2

        var act = async () => await new ConfirmSeparationStockistWithdrawalHandler(repo, stockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<StockQuantityConflictException>();
    }

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenInsufficientReservedStock_DoesNotChangeSeparationStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync(stockQuantity: 10m);
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new ReserveSeparationOrderHandler(repo, stockRepo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var partId = command.Parts[0].PartId;
        var stock = await stockRepo.GetByItemAsync(partId, StockItemType.Part);
        stock!.Release(1m);

        try { await new ConfirmSeparationStockistWithdrawalHandler(repo, stockRepo).HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid())); }
        catch (StockQuantityConflictException) { }

        var separation = await repo.GetByIdAsync(dto.Id);
        separation!.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenNotWaitingPickup_ThrowsInvalidSeparationOrderStatusTransitionException()
    {
        var repo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var act = async () => await new ConfirmSeparationStockistWithdrawalHandler(repo, stockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>();
    }

    // --- ReturnSeparationOrderTotalHandler ---

    [Fact]
    public async Task ReturnTotal_WhenSeparated_ReturnsPendingStatusAndReleasesStock()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync(stockQuantity: 50m);
        var createDto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new ReserveSeparationOrderHandler(repo, stockRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(createDto.Id));
        await new ConfirmSeparationStockistWithdrawalHandler(repo, stockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(createDto.Id, Guid.NewGuid()));

        var result = await new ReturnSeparationOrderTotalHandler(repo, stockRepo)
            .HandleAsync(new ReturnSeparationOrderTotalCommand(createDto.Id));

        result.Status.Should().Be(SeparationOrderStatus.Pending);
        result.StockistId.Should().BeNull();
        result.ConfirmedByStockistAt.Should().BeNull();

        var partId = command.Parts[0].PartId;
        var stock = await stockRepo.GetByItemAsync(partId, StockItemType.Part);
        stock!.ReservedQuantity.Should().Be(0m);
        stock.AvailableQuantity.Should().Be(50m);
    }

    [Fact]
    public async Task ReturnTotal_WhenNotSeparated_ThrowsSeparationOrderCustodyPreconditionException()
    {
        var repo = new FakeSeparationOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync();
        var createDto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);

        var act = async () => await new ReturnSeparationOrderTotalHandler(repo, stockRepo)
            .HandleAsync(new ReturnSeparationOrderTotalCommand(createDto.Id));

        await act.Should().ThrowAsync<SeparationOrderCustodyPreconditionException>();
    }

    [Fact]
    public async Task ReturnTotal_WhenNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();

        var act = async () => await new ReturnSeparationOrderTotalHandler(repo, stockRepo)
            .HandleAsync(new ReturnSeparationOrderTotalCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- ConfirmSeparationMechanicReceiptHandler ---

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenSeparated_ReturnsCompletedStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var executionRepo = new FakeExecutionOrderRepository();
        var executionOrder = await AddPendingExecutionOrder(executionRepo);
        var (command, stockRepo) = await BuildCommandWithStockAsync(executionOrder.Id);
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);

        await new ReserveSeparationOrderHandler(repo, stockRepo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));
        await new ConfirmSeparationStockistWithdrawalHandler(repo, stockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        var result = await new ConfirmSeparationMechanicReceiptHandler(repo, executionRepo)
            .HandleAsync(new ConfirmSeparationMechanicReceiptCommand(dto.Id));

        result.Status.Should().Be(SeparationOrderStatus.Completed);
        result.ConfirmedByMechanicAt.Should().NotBeNull();
        executionOrder.Status.Should().Be(ExecutionOrderStatus.Ready);
    }

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenNotSeparated_ThrowsInvalidSeparationOrderStatusTransitionException()
    {
        var repo = new FakeSeparationOrderRepository();
        var executionRepo = new FakeExecutionOrderRepository();
        var executionOrder = await AddPendingExecutionOrder(executionRepo);
        var (command, stockRepo) = await BuildCommandWithStockAsync(executionOrder.Id);
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);
        await new ReserveSeparationOrderHandler(repo, stockRepo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var act = async () => await new ConfirmSeparationMechanicReceiptHandler(repo, executionRepo)
            .HandleAsync(new ConfirmSeparationMechanicReceiptCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>();
    }

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSeparationOrderRepository();
        var executionRepo = new FakeExecutionOrderRepository();
        var handler = new ConfirmSeparationMechanicReceiptHandler(repo, executionRepo);

        var act = async () => await handler.HandleAsync(new ConfirmSeparationMechanicReceiptCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenExecutionOrderNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSeparationOrderRepository();
        var executionRepo = new FakeExecutionOrderRepository();
        var (command, stockRepo) = await BuildCommandWithStockAsync();
        var dto = await new CreateSeparationOrderHandler(repo).HandleAsync(command);

        await new ReserveSeparationOrderHandler(repo, stockRepo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));
        await new ConfirmSeparationStockistWithdrawalHandler(repo, stockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        var act = async () => await new ConfirmSeparationMechanicReceiptHandler(repo, executionRepo)
            .HandleAsync(new ConfirmSeparationMechanicReceiptCommand(dto.Id));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
