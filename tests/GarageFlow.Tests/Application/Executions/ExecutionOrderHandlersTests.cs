using FluentAssertions;
using GarageFlow.Application.Executions;
using GarageFlow.Application.Executions.Commands;
using GarageFlow.Application.Executions.Handlers;
using GarageFlow.Application.Executions.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Tests.Application.ServiceOrders;

namespace GarageFlow.Tests.Application.Executions;

public sealed class ExecutionOrderHandlersTests
{
    private static CreateExecutionOrderCommand ValidCreateCommand(
        Guid? serviceOrderId = null,
        Guid? serviceId = null) =>
        new(serviceOrderId ?? Guid.NewGuid(), serviceId ?? Guid.NewGuid());

    // --- CreateExecutionOrderHandler ---

    [Fact]
    public async Task CreateExecutionOrder_WithValidData_ReturnsDtoWithStatusPending()
    {
        var repo = new FakeExecutionOrderRepository();
        var handler = new CreateExecutionOrderHandler(repo);
        var command = ValidCreateCommand();

        var dto = await handler.HandleAsync(command);

        dto.Should().NotBeNull();
        dto.Id.Should().NotBeEmpty();
        dto.ServiceOrderId.Should().Be(command.ServiceOrderId);
        dto.ServiceId.Should().Be(command.ServiceId);
        dto.Status.Should().Be(ExecutionOrderStatus.Pending);
        dto.MechanicId.Should().BeNull();
        dto.StartedAt.Should().BeNull();
        dto.CompletedAt.Should().BeNull();
        dto.ActualTimeMinutes.Should().BeNull();
    }

    [Fact]
    public async Task CreateExecutionOrder_WithEmptyServiceOrderId_ThrowsDomainException()
    {
        var repo = new FakeExecutionOrderRepository();
        var handler = new CreateExecutionOrderHandler(repo);
        var command = ValidCreateCommand(serviceOrderId: Guid.Empty);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>().WithMessage("OS é obrigatória");
    }

    [Fact]
    public async Task CreateExecutionOrder_WithEmptyServiceId_ThrowsDomainException()
    {
        var repo = new FakeExecutionOrderRepository();
        var handler = new CreateExecutionOrderHandler(repo);
        var command = ValidCreateCommand(serviceId: Guid.Empty);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Serviço é obrigatório");
    }

    // --- GetExecutionOrderByIdHandler ---

    [Fact]
    public async Task GetExecutionOrderById_WhenExists_ReturnsDto()
    {
        var repo = new FakeExecutionOrderRepository();
        var createHandler = new CreateExecutionOrderHandler(repo);
        var created = await createHandler.HandleAsync(ValidCreateCommand());

        var getHandler = new GetExecutionOrderByIdHandler(repo);
        var result = await getHandler.HandleAsync(new GetExecutionOrderByIdQuery(created.Id));

        result.Should().NotBeNull();
        result.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetExecutionOrderById_WhenNotExists_ThrowsEntityNotFoundException()
    {
        var repo = new FakeExecutionOrderRepository();
        var handler = new GetExecutionOrderByIdHandler(repo);

        var act = async () => await handler.HandleAsync(new GetExecutionOrderByIdQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- ListExecutionOrdersHandler ---

    [Fact]
    public async Task ListExecutionOrders_WithValidPagination_ReturnsPagedResult()
    {
        var repo = new FakeExecutionOrderRepository();
        var createHandler = new CreateExecutionOrderHandler(repo);
        await createHandler.HandleAsync(ValidCreateCommand());
        await createHandler.HandleAsync(ValidCreateCommand());

        var listHandler = new ListExecutionOrdersHandler(repo);
        var result = await listHandler.HandleAsync(new ListExecutionOrdersQuery(1, 10));

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task ListExecutionOrders_WithInvalidPage_ThrowsDomainException()
    {
        var repo = new FakeExecutionOrderRepository();
        var handler = new ListExecutionOrdersHandler(repo);

        var act = async () => await handler.HandleAsync(new ListExecutionOrdersQuery(0, 10));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task ListExecutionOrders_WithPageSizeAboveMax_ThrowsDomainException()
    {
        var repo = new FakeExecutionOrderRepository();
        var handler = new ListExecutionOrdersHandler(repo);

        var act = async () => await handler.HandleAsync(
            new ListExecutionOrdersQuery(1, ExecutionOrderPaginationDefaults.MaxPageSize + 1));

        await act.Should().ThrowAsync<DomainException>();
    }

    // --- MarkExecutionOrderReadyHandler ---

    [Fact]
    public async Task MarkExecutionOrderReady_WhenPending_ChangesStatusToReady()
    {
        var repo = new FakeExecutionOrderRepository();
        var createHandler = new CreateExecutionOrderHandler(repo);
        var created = await createHandler.HandleAsync(ValidCreateCommand());

        var markReadyHandler = new MarkExecutionOrderReadyHandler(repo);
        var result = await markReadyHandler.HandleAsync(new MarkExecutionOrderReadyCommand(created.Id));

        result.Status.Should().Be(ExecutionOrderStatus.Ready);
    }

    [Fact]
    public async Task MarkExecutionOrderReady_CalledTwice_IsIdempotent()
    {
        var repo = new FakeExecutionOrderRepository();
        var createHandler = new CreateExecutionOrderHandler(repo);
        var created = await createHandler.HandleAsync(ValidCreateCommand());
        var markReadyHandler = new MarkExecutionOrderReadyHandler(repo);
        await markReadyHandler.HandleAsync(new MarkExecutionOrderReadyCommand(created.Id));

        var act = async () => await markReadyHandler.HandleAsync(new MarkExecutionOrderReadyCommand(created.Id));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task MarkExecutionOrderReady_WhenNotExists_ThrowsEntityNotFoundException()
    {
        var repo = new FakeExecutionOrderRepository();
        var handler = new MarkExecutionOrderReadyHandler(repo);

        var act = async () => await handler.HandleAsync(new MarkExecutionOrderReadyCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- StartExecutionOrderHandler ---

    [Fact]
    public async Task StartExecutionOrder_WhenReady_ChangesStatusToInExecution()
    {
        var repo = new FakeExecutionOrderRepository();
        var createHandler = new CreateExecutionOrderHandler(repo);
        var created = await createHandler.HandleAsync(ValidCreateCommand());
        await new MarkExecutionOrderReadyHandler(repo).HandleAsync(new MarkExecutionOrderReadyCommand(created.Id));

        var mechanicId = Guid.NewGuid();
        var startHandler = new StartExecutionOrderHandler(repo);
        var result = await startHandler.HandleAsync(new StartExecutionOrderCommand(created.Id, mechanicId));

        result.Status.Should().Be(ExecutionOrderStatus.InExecution);
        result.MechanicId.Should().Be(mechanicId);
        result.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task StartExecutionOrder_WhenPending_ThrowsInvalidExecutionOrderStatusTransitionException()
    {
        var repo = new FakeExecutionOrderRepository();
        var createHandler = new CreateExecutionOrderHandler(repo);
        var created = await createHandler.HandleAsync(ValidCreateCommand());

        var startHandler = new StartExecutionOrderHandler(repo);
        var act = async () => await startHandler.HandleAsync(new StartExecutionOrderCommand(created.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<InvalidExecutionOrderStatusTransitionException>();
    }

    [Fact]
    public async Task StartExecutionOrder_WithEmptyMechanicId_ThrowsDomainException()
    {
        var repo = new FakeExecutionOrderRepository();
        var createHandler = new CreateExecutionOrderHandler(repo);
        var created = await createHandler.HandleAsync(ValidCreateCommand());
        await new MarkExecutionOrderReadyHandler(repo).HandleAsync(new MarkExecutionOrderReadyCommand(created.Id));

        var startHandler = new StartExecutionOrderHandler(repo);
        var act = async () => await startHandler.HandleAsync(new StartExecutionOrderCommand(created.Id, Guid.Empty));

        await act.Should().ThrowAsync<DomainException>().WithMessage("Mecânico é obrigatório");
    }

    [Fact]
    public async Task StartExecutionOrder_WhenNotExists_ThrowsEntityNotFoundException()
    {
        var repo = new FakeExecutionOrderRepository();
        var handler = new StartExecutionOrderHandler(repo);

        var act = async () => await handler.HandleAsync(new StartExecutionOrderCommand(Guid.NewGuid(), Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- CompleteExecutionOrderHandler ---

    [Fact]
    public async Task CompleteExecutionOrder_WhenInExecution_ChangesStatusToCompleted()
    {
        var repo = new FakeExecutionOrderRepository();
        var soRepo = new FakeServiceOrderRepository();

        var serviceOrderId = Guid.NewGuid();
        var so = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        typeof(ServiceOrder).GetProperty(nameof(ServiceOrder.Id))!.SetValue(so, serviceOrderId);
        typeof(ServiceOrder).GetProperty(nameof(ServiceOrder.Status))!.SetValue(so, ServiceOrderStatus.InExecution);
        await soRepo.AddAsync(so);

        var createHandler = new CreateExecutionOrderHandler(repo);
        var created = await createHandler.HandleAsync(new CreateExecutionOrderCommand(serviceOrderId, Guid.NewGuid()));
        await new MarkExecutionOrderReadyHandler(repo).HandleAsync(new MarkExecutionOrderReadyCommand(created.Id));
        await new StartExecutionOrderHandler(repo).HandleAsync(new StartExecutionOrderCommand(created.Id, Guid.NewGuid()));

        var completeHandler = new CompleteExecutionOrderHandler(repo, soRepo);
        var result = await completeHandler.HandleAsync(new CompleteExecutionOrderCommand(created.Id));

        result.Status.Should().Be(ExecutionOrderStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
        result.ActualTimeMinutes.Should().NotBeNull();
        result.ActualTimeMinutes!.Value.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task CompleteExecutionOrder_WhenPending_ThrowsInvalidExecutionOrderStatusTransitionException()
    {
        var repo = new FakeExecutionOrderRepository();
        var createHandler = new CreateExecutionOrderHandler(repo);
        var created = await createHandler.HandleAsync(ValidCreateCommand());

        var soRepo = new FakeServiceOrderRepository();
        var completeHandler = new CompleteExecutionOrderHandler(repo, soRepo);
        var act = async () => await completeHandler.HandleAsync(new CompleteExecutionOrderCommand(created.Id));

        await act.Should().ThrowAsync<InvalidExecutionOrderStatusTransitionException>();
    }

    [Fact]
    public async Task CompleteExecutionOrder_WhenNotExists_ThrowsEntityNotFoundException()
    {
        var repo = new FakeExecutionOrderRepository();
        var soRepo = new FakeServiceOrderRepository();
        var handler = new CompleteExecutionOrderHandler(repo, soRepo);

        var act = async () => await handler.HandleAsync(new CompleteExecutionOrderCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
