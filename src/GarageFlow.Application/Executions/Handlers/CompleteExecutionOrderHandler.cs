using GarageFlow.Application.Executions.Commands;
using GarageFlow.Application.Executions.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Executions.Handlers;

public sealed class CompleteExecutionOrderHandler(
    IExecutionOrderRepository executionOrderRepository,
    IServiceOrderRepository serviceOrderRepository,
    ISeparationOrderRepository separationOrderRepository,
    IStockRepository stockRepository)
{
    public async Task<ExecutionOrderDto> HandleAsync(CompleteExecutionOrderCommand command, CancellationToken cancellationToken = default)
    {
        var executionOrder = await executionOrderRepository.GetByIdAsync(command.ExecutionOrderId, cancellationToken);
        if (executionOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ExecutionOrderNotFound(command.ExecutionOrderId));

        var separationOrder = await separationOrderRepository.GetByExecutionOrderIdAsync(executionOrder.Id, cancellationToken);
        if (separationOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFoundForExecution(executionOrder.Id));
        if (separationOrder.Status != SeparationOrderStatus.Completed)
            throw new SeparationOrderCustodyPreconditionException(DomainErrorMessages.SeparationOrderNotCompletedForExecution);

        foreach (var part in separationOrder.Parts)
        {
            var stock = await stockRepository.GetByItemAsync(part.PartId, StockItemType.Part, cancellationToken);
            if (stock is null)
                throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(StockItemType.Part, part.PartId));

            stock.Consume(part.Quantity, referenceId: executionOrder.Id);
            stockRepository.Update(stock);
        }

        foreach (var supply in separationOrder.Supplies)
        {
            var stock = await stockRepository.GetByItemAsync(supply.SupplyId, StockItemType.Supply, cancellationToken);
            if (stock is null)
                throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(StockItemType.Supply, supply.SupplyId));

            stock.Consume(supply.Quantity, referenceId: executionOrder.Id);
            stockRepository.Update(stock);
        }

        executionOrder.CompleteExecution();

        var siblingsOrders = await executionOrderRepository.GetByServiceOrderIdAsync(executionOrder.ServiceOrderId, cancellationToken);

        var allCompleted = siblingsOrders.All(eo => eo.Status == ExecutionOrderStatus.Completed);

        if (allCompleted)
        {
            var serviceOrder = await serviceOrderRepository.GetByIdAsync(executionOrder.ServiceOrderId, cancellationToken);
            if (serviceOrder is null)
                throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(executionOrder.ServiceOrderId));

            serviceOrder.Finish();
        }

        await executionOrderRepository.SaveChangesAsync(cancellationToken);

        return ExecutionOrderMapper.ToDto(executionOrder);
    }
}
