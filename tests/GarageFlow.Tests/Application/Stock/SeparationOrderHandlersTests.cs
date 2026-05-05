using FluentAssertions;
using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.Handlers;
using GarageFlow.Application.Stock.Queries;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;
using GarageFlow.Tests.Application.Executions;

namespace GarageFlow.Tests.Application.Stock;

public sealed class SeparationOrderHandlersTests
{
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
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var reserveHandler = new ReserveSeparationOrderHandler(repo);
        var result = await reserveHandler.HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        result.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    [Fact]
    public async Task Reserve_WhenNotPending_ThrowsInvalidSeparationOrderStatusTransitionException()
    {
        var repo = new FakeSeparationOrderRepository();
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var reserveHandler = new ReserveSeparationOrderHandler(repo);
        await reserveHandler.HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var act = async () => await reserveHandler.HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>();
    }

    [Fact]
    public async Task Reserve_WhenNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSeparationOrderRepository();
        var handler = new ReserveSeparationOrderHandler(repo);

        var act = async () => await handler.HandleAsync(new ReserveSeparationOrderCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
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
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var reserveHandler = new ReserveSeparationOrderHandler(repo);
        await reserveHandler.HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var waitHandler = new WaitSeparationOrderPurchaseHandler(repo);
        var act = async () => await waitHandler.HandleAsync(new WaitSeparationOrderPurchaseCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>();
    }

    // --- ResumeSeparationOrderAfterPurchaseHandler ---

    [Fact]
    public async Task ResumeAfterPurchase_WhenWaitingPurchase_ReturnsWaitingPickupStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var waitHandler = new WaitSeparationOrderPurchaseHandler(repo);
        await waitHandler.HandleAsync(new WaitSeparationOrderPurchaseCommand(dto.Id));

        var resumeHandler = new ResumeSeparationOrderAfterPurchaseHandler(repo);
        var result = await resumeHandler.HandleAsync(new ResumeSeparationOrderAfterPurchaseCommand(dto.Id));

        result.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    [Fact]
    public async Task ResumeAfterPurchase_WhenNotWaitingPurchase_ThrowsInvalidSeparationOrderStatusTransitionException()
    {
        var repo = new FakeSeparationOrderRepository();
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var resumeHandler = new ResumeSeparationOrderAfterPurchaseHandler(repo);
        var act = async () => await resumeHandler.HandleAsync(new ResumeSeparationOrderAfterPurchaseCommand(dto.Id));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>();
    }

    // --- ConfirmSeparationStockistWithdrawalHandler ---

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenWaitingPickup_ReturnsSeparatedStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var reserveHandler = new ReserveSeparationOrderHandler(repo);
        await reserveHandler.HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var confirmHandler = new ConfirmSeparationStockistWithdrawalHandler(repo);
        var result = await confirmHandler.HandleAsync(
            new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        result.Status.Should().Be(SeparationOrderStatus.Separated);
        result.StockistId.Should().NotBeNull();
        result.ConfirmedByStockistAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenNotWaitingPickup_ThrowsInvalidSeparationOrderStatusTransitionException()
    {
        var repo = new FakeSeparationOrderRepository();
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand());

        var confirmHandler = new ConfirmSeparationStockistWithdrawalHandler(repo);
        var act = async () => await confirmHandler.HandleAsync(
            new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<InvalidSeparationOrderStatusTransitionException>();
    }

    // --- ConfirmSeparationMechanicReceiptHandler ---

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenSeparated_ReturnsCompletedStatus()
    {
        var repo = new FakeSeparationOrderRepository();
        var executionRepo = new FakeExecutionOrderRepository();
        var executionOrder = await AddPendingExecutionOrder(executionRepo);
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand(executionOrder.Id));

        await new ReserveSeparationOrderHandler(repo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));
        await new ConfirmSeparationStockistWithdrawalHandler(repo).HandleAsync(
            new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        var confirmHandler = new ConfirmSeparationMechanicReceiptHandler(repo, executionRepo);
        var result = await confirmHandler.HandleAsync(new ConfirmSeparationMechanicReceiptCommand(dto.Id));

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
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand(executionOrder.Id));

        await new ReserveSeparationOrderHandler(repo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));

        var confirmHandler = new ConfirmSeparationMechanicReceiptHandler(repo, executionRepo);
        var act = async () => await confirmHandler.HandleAsync(new ConfirmSeparationMechanicReceiptCommand(dto.Id));

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
        var createHandler = new CreateSeparationOrderHandler(repo);
        var dto = await createHandler.HandleAsync(ValidCreateCommand(Guid.NewGuid()));

        await new ReserveSeparationOrderHandler(repo).HandleAsync(new ReserveSeparationOrderCommand(dto.Id));
        await new ConfirmSeparationStockistWithdrawalHandler(repo).HandleAsync(
            new ConfirmSeparationStockistWithdrawalCommand(dto.Id, Guid.NewGuid()));

        var handler = new ConfirmSeparationMechanicReceiptHandler(repo, executionRepo);
        var act = async () => await handler.HandleAsync(new ConfirmSeparationMechanicReceiptCommand(dto.Id));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
