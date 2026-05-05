using FluentAssertions;
using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.Handlers;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Stock;
using GarageFlow.Tests.Application.Executions;

namespace GarageFlow.Tests.Application.Stock;

public sealed class SeparationExecutionIntegrationTests
{
    [Fact]
    public async Task ConfirmMechanicReceipt_CompletesSeparation_AndMarksExecutionReady()
    {
        var separationRepo = new FakeSeparationOrderRepository();
        var executionRepo = new FakeExecutionOrderRepository();

        var execution = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await executionRepo.AddAsync(execution);

        var createSeparation = new CreateSeparationOrderHandler(separationRepo);
        var separation = await createSeparation.HandleAsync(
            new CreateSeparationOrderCommand(
                execution.Id,
                [new CreateSeparationPartItemCommand(Guid.NewGuid(), "Filtro de óleo", 1)],
                []));

        await new ReserveSeparationOrderHandler(separationRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(separation.Id));
        await new ConfirmSeparationStockistWithdrawalHandler(separationRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(separation.Id, Guid.NewGuid()));

        var handler = new ConfirmSeparationMechanicReceiptHandler(separationRepo, executionRepo);
        var result = await handler.HandleAsync(new ConfirmSeparationMechanicReceiptCommand(separation.Id));

        result.Status.Should().Be(SeparationOrderStatus.Completed);
        execution.Status.Should().Be(ExecutionOrderStatus.Ready);
    }

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenExecutionMissing_ThrowsNotFound()
    {
        var separationRepo = new FakeSeparationOrderRepository();
        var executionRepo = new FakeExecutionOrderRepository();

        var createSeparation = new CreateSeparationOrderHandler(separationRepo);
        var separation = await createSeparation.HandleAsync(
            new CreateSeparationOrderCommand(
                Guid.NewGuid(),
                [new CreateSeparationPartItemCommand(Guid.NewGuid(), "Filtro de óleo", 1)],
                []));

        await new ReserveSeparationOrderHandler(separationRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(separation.Id));
        await new ConfirmSeparationStockistWithdrawalHandler(separationRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(separation.Id, Guid.NewGuid()));

        var handler = new ConfirmSeparationMechanicReceiptHandler(separationRepo, executionRepo);
        var act = async () => await handler.HandleAsync(new ConfirmSeparationMechanicReceiptCommand(separation.Id));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
