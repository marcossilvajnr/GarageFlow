using FluentAssertions;
using GarageFlow.Application.Executions.Commands;
using GarageFlow.Application.Executions.Handlers;
using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.Handlers;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Stock;
using GarageFlow.Tests.Application.Executions;
using GarageFlow.Tests.Application.ServiceOrders;
using DomainStock = GarageFlow.Domain.Stock.Stock;

namespace GarageFlow.Tests.Application.Stock;

public sealed class SeparationExecutionIntegrationTests
{
    [Fact]
    public async Task ConfirmMechanicReceipt_CompletesSeparation_AndMarksExecutionReady()
    {
        var separationRepo = new FakeSeparationOrderRepository();
        var executionRepo = new FakeExecutionOrderRepository();
        var stockRepo = new FakeStockRepository();

        var execution = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await executionRepo.AddAsync(execution);

        var partId = Guid.NewGuid();
        var stock = DomainStock.Create(partId, StockItemType.Part, 100m, 0m);
        await stockRepo.AddAsync(stock);

        var createSeparation = new CreateSeparationOrderHandler(separationRepo);
        var separation = await createSeparation.HandleAsync(
            new CreateSeparationOrderCommand(
                execution.Id,
                [new CreateSeparationPartItemCommand(partId, "Filtro de óleo", 1)],
                []));

        await new ReserveSeparationOrderHandler(separationRepo, stockRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(separation.Id));
        await new ConfirmSeparationStockistWithdrawalHandler(separationRepo, stockRepo)
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
        var stockRepo = new FakeStockRepository();

        var partId = Guid.NewGuid();
        var stock = DomainStock.Create(partId, StockItemType.Part, 100m, 0m);
        await stockRepo.AddAsync(stock);

        var createSeparation = new CreateSeparationOrderHandler(separationRepo);
        var separation = await createSeparation.HandleAsync(
            new CreateSeparationOrderCommand(
                Guid.NewGuid(),
                [new CreateSeparationPartItemCommand(partId, "Filtro de óleo", 1)],
                []));

        await new ReserveSeparationOrderHandler(separationRepo, stockRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(separation.Id));
        await new ConfirmSeparationStockistWithdrawalHandler(separationRepo, stockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(separation.Id, Guid.NewGuid()));

        var handler = new ConfirmSeparationMechanicReceiptHandler(separationRepo, executionRepo);
        var act = async () => await handler.HandleAsync(new ConfirmSeparationMechanicReceiptCommand(separation.Id));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task StockBalance_OnlyDecreasesAtConfirmStockistWithdrawal_NotAtCompleteExecution()
    {
        // Arrange
        var separationRepo = new FakeSeparationOrderRepository();
        var executionRepo = new FakeExecutionOrderRepository();
        var stockRepo = new FakeStockRepository();
        var soRepo = new FakeServiceOrderRepository();

        var so = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        typeof(ServiceOrder).GetProperty(nameof(ServiceOrder.Status))!
            .SetValue(so, ServiceOrderStatus.InExecution);
        await soRepo.AddAsync(so);

        var execution = ExecutionOrder.Create(so.Id, Guid.NewGuid());
        await executionRepo.AddAsync(execution);

        var partId = Guid.NewGuid();
        var stock = DomainStock.Create(partId, StockItemType.Part, 10m, 0m);
        await stockRepo.AddAsync(stock);

        var createSeparation = new CreateSeparationOrderHandler(separationRepo);
        var separation = await createSeparation.HandleAsync(
            new CreateSeparationOrderCommand(
                execution.Id,
                [new CreateSeparationPartItemCommand(partId, "Filtro de óleo", 1)],
                []));

        // Reserve
        await new ReserveSeparationOrderHandler(separationRepo, stockRepo)
            .HandleAsync(new ReserveSeparationOrderCommand(separation.Id));

        var totalAfterReserve = stock.TotalQuantity;
        var availableAfterReserve = stock.AvailableQuantity;
        var reservedAfterReserve = stock.ReservedQuantity;

        // Baixa definitiva ocorre aqui — ConfirmStockistWithdrawal
        await new ConfirmSeparationStockistWithdrawalHandler(separationRepo, stockRepo)
            .HandleAsync(new ConfirmSeparationStockistWithdrawalCommand(separation.Id, Guid.NewGuid()));

        var totalAfterWithdrawal = stock.TotalQuantity;
        var reservedAfterWithdrawal = stock.ReservedQuantity;

        // Confirmar recebimento do mecânico e marcar execução como pronta
        await new ConfirmSeparationMechanicReceiptHandler(separationRepo, executionRepo)
            .HandleAsync(new ConfirmSeparationMechanicReceiptCommand(separation.Id));

        // Avançar execução para InExecution
        await new StartExecutionOrderHandler(executionRepo, soRepo)
            .HandleAsync(new StartExecutionOrderCommand(execution.Id, Guid.NewGuid()));

        var totalBeforeComplete = stock.TotalQuantity;
        var reservedBeforeComplete = stock.ReservedQuantity;
        var availableBeforeComplete = stock.AvailableQuantity;

        // CompleteExecution NÃO deve alterar saldo de estoque
        await new CompleteExecutionOrderHandler(executionRepo, soRepo, separationRepo)
            .HandleAsync(new CompleteExecutionOrderCommand(execution.Id));

        // Assert: reserva foi consumida na retirada do estoquista
        totalAfterWithdrawal.Should().Be(totalAfterReserve - 1m, "a baixa ocorre em ConfirmStockistWithdrawal");
        reservedAfterWithdrawal.Should().Be(reservedAfterReserve - 1m, "a reserva é zerada na baixa");

        // Assert: CompleteExecution não altera saldo
        stock.TotalQuantity.Should().Be(totalBeforeComplete, "CompleteExecution não deve baixar estoque");
        stock.ReservedQuantity.Should().Be(reservedBeforeComplete, "CompleteExecution não deve liberar reserva");
        stock.AvailableQuantity.Should().Be(availableBeforeComplete, "CompleteExecution não deve alterar disponível");
    }
}
