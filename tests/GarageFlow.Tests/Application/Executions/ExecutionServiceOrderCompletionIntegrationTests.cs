using FluentAssertions;
using GarageFlow.Application.Executions.Commands;
using GarageFlow.Application.Executions.Handlers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Stock;
using GarageFlow.Tests.Application.ServiceOrders;
using GarageFlow.Tests.Application.Stock;
using DomainStock = GarageFlow.Domain.Stock.Stock;

namespace GarageFlow.Tests.Application.Executions;

public sealed class ExecutionServiceOrderCompletionIntegrationTests
{
    private static ServiceOrder CreateServiceOrderInExecution()
    {
        var so = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        typeof(ServiceOrder)
            .GetProperty(nameof(ServiceOrder.Status))!
            .SetValue(so, ServiceOrderStatus.InExecution);
        return so;
    }

    private static async Task AdvanceToInExecution(FakeExecutionOrderRepository execRepo, Guid executionOrderId)
    {
        await new MarkExecutionOrderReadyHandler(execRepo).HandleAsync(new MarkExecutionOrderReadyCommand(executionOrderId));
        await new StartExecutionOrderHandler(execRepo).HandleAsync(new StartExecutionOrderCommand(executionOrderId, Guid.NewGuid()));
    }

    private static async Task SeedSeparationAndStock(
        FakeSeparationOrderRepository separationRepo,
        FakeStockRepository stockRepo,
        Guid executionOrderId)
    {
        var partId = Guid.NewGuid();
        var stock = DomainStock.Create(partId, StockItemType.Part, 10m, 0m);
        stock.Reserve(1m);
        await stockRepo.AddAsync(stock);

        var separation = SeparationOrder.Create(
            executionOrderId,
            [SeparationPartItem.Create(partId, "Filtro de óleo", 1)],
            []);
        typeof(SeparationOrder).GetProperty(nameof(SeparationOrder.Status))!
            .SetValue(separation, SeparationOrderStatus.Completed);
        await separationRepo.AddAsync(separation);
    }

    // --- Conclusão da execução intermediária mantém OS aberta ---

    [Fact]
    public async Task CompleteExecution_WhenSiblingExecutionsRemain_DoesNotFinishServiceOrder()
    {
        var execRepo = new FakeExecutionOrderRepository();
        var soRepo = new FakeServiceOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var so = CreateServiceOrderInExecution();
        await soRepo.AddAsync(so);

        var createHandler = new CreateExecutionOrderHandler(execRepo);
        var first = await createHandler.HandleAsync(new CreateExecutionOrderCommand(so.Id, Guid.NewGuid()));
        var second = await createHandler.HandleAsync(new CreateExecutionOrderCommand(so.Id, Guid.NewGuid()));

        await AdvanceToInExecution(execRepo, first.Id);
        await AdvanceToInExecution(execRepo, second.Id);
        await SeedSeparationAndStock(separationRepo, stockRepo, first.Id);

        var handler = new CompleteExecutionOrderHandler(execRepo, soRepo, separationRepo);
        await handler.HandleAsync(new CompleteExecutionOrderCommand(first.Id));

        var reloadedSo = await soRepo.GetByIdAsync(so.Id);
        reloadedSo!.Status.Should().Be(ServiceOrderStatus.InExecution);
    }

    // --- Conclusão da última execução fecha OS ---

    [Fact]
    public async Task CompleteExecution_WhenLastExecution_FinishesServiceOrder()
    {
        var execRepo = new FakeExecutionOrderRepository();
        var soRepo = new FakeServiceOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var so = CreateServiceOrderInExecution();
        await soRepo.AddAsync(so);

        var createHandler = new CreateExecutionOrderHandler(execRepo);
        var created = await createHandler.HandleAsync(new CreateExecutionOrderCommand(so.Id, Guid.NewGuid()));

        await AdvanceToInExecution(execRepo, created.Id);
        await SeedSeparationAndStock(separationRepo, stockRepo, created.Id);

        var handler = new CompleteExecutionOrderHandler(execRepo, soRepo, separationRepo);
        await handler.HandleAsync(new CompleteExecutionOrderCommand(created.Id));

        var reloadedSo = await soRepo.GetByIdAsync(so.Id);
        reloadedSo!.Status.Should().Be(ServiceOrderStatus.Finished);
    }

    [Fact]
    public async Task CompleteExecution_WhenAllSiblingsAlreadyCompleted_FinishesServiceOrder()
    {
        var execRepo = new FakeExecutionOrderRepository();
        var soRepo = new FakeServiceOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();
        var so = CreateServiceOrderInExecution();
        await soRepo.AddAsync(so);

        var createHandler = new CreateExecutionOrderHandler(execRepo);
        var first = await createHandler.HandleAsync(new CreateExecutionOrderCommand(so.Id, Guid.NewGuid()));
        var second = await createHandler.HandleAsync(new CreateExecutionOrderCommand(so.Id, Guid.NewGuid()));

        await AdvanceToInExecution(execRepo, first.Id);
        await AdvanceToInExecution(execRepo, second.Id);
        await SeedSeparationAndStock(separationRepo, stockRepo, first.Id);
        await SeedSeparationAndStock(separationRepo, stockRepo, second.Id);

        var handler = new CompleteExecutionOrderHandler(execRepo, soRepo, separationRepo);

        // Complete first — SO should remain InExecution
        await handler.HandleAsync(new CompleteExecutionOrderCommand(first.Id));
        var soAfterFirst = await soRepo.GetByIdAsync(so.Id);
        soAfterFirst!.Status.Should().Be(ServiceOrderStatus.InExecution);

        // Complete second (last) — SO should transition to Finished
        await handler.HandleAsync(new CompleteExecutionOrderCommand(second.Id));
        var soAfterLast = await soRepo.GetByIdAsync(so.Id);
        soAfterLast!.Status.Should().Be(ServiceOrderStatus.Finished);
    }

    // --- Execução vinculada a OS inexistente retorna erro ---

    [Fact]
    public async Task CompleteExecution_WhenServiceOrderNotFound_ThrowsEntityNotFoundException()
    {
        var execRepo = new FakeExecutionOrderRepository();
        var soRepo = new FakeServiceOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();

        var createHandler = new CreateExecutionOrderHandler(execRepo);
        var created = await createHandler.HandleAsync(new CreateExecutionOrderCommand(Guid.NewGuid(), Guid.NewGuid()));

        await new MarkExecutionOrderReadyHandler(execRepo).HandleAsync(new MarkExecutionOrderReadyCommand(created.Id));
        await new StartExecutionOrderHandler(execRepo).HandleAsync(new StartExecutionOrderCommand(created.Id, Guid.NewGuid()));
        await SeedSeparationAndStock(separationRepo, stockRepo, created.Id);

        var handler = new CompleteExecutionOrderHandler(execRepo, soRepo, separationRepo);
        var act = async () => await handler.HandleAsync(new CompleteExecutionOrderCommand(created.Id));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- Execução inexistente retorna erro ---

    [Fact]
    public async Task CompleteExecution_WhenExecutionNotFound_ThrowsEntityNotFoundException()
    {
        var execRepo = new FakeExecutionOrderRepository();
        var soRepo = new FakeServiceOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var handler = new CompleteExecutionOrderHandler(execRepo, soRepo, separationRepo);

        var act = async () => await handler.HandleAsync(new CompleteExecutionOrderCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // --- Transição inválida retorna conflito ---

    [Fact]
    public async Task CompleteExecution_WhenNotInExecution_ThrowsInvalidExecutionOrderStatusTransitionException()
    {
        var execRepo = new FakeExecutionOrderRepository();
        var soRepo = new FakeServiceOrderRepository();
        var separationRepo = new FakeSeparationOrderRepository();
        var stockRepo = new FakeStockRepository();

        var createHandler = new CreateExecutionOrderHandler(execRepo);
        var created = await createHandler.HandleAsync(new CreateExecutionOrderCommand(Guid.NewGuid(), Guid.NewGuid()));
        await SeedSeparationAndStock(separationRepo, stockRepo, created.Id);

        var handler = new CompleteExecutionOrderHandler(execRepo, soRepo, separationRepo);
        var act = async () => await handler.HandleAsync(new CompleteExecutionOrderCommand(created.Id));

        await act.Should().ThrowAsync<InvalidExecutionOrderStatusTransitionException>();
    }
}
