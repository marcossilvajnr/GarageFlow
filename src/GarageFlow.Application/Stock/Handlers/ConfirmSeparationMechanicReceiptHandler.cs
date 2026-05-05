using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class ConfirmSeparationMechanicReceiptHandler(
    ISeparationOrderRepository separationOrderRepository,
    IExecutionOrderRepository executionOrderRepository)
{
    public async Task<SeparationOrderDto> HandleAsync(ConfirmSeparationMechanicReceiptCommand command, CancellationToken cancellationToken = default)
    {
        var separationOrder = await separationOrderRepository.GetByIdAsync(command.SeparationOrderId, cancellationToken);
        if (separationOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFound(command.SeparationOrderId));

        separationOrder.ConfirmMechanicReceipt();

        var executionOrder = await executionOrderRepository.GetByIdAsync(separationOrder.ExecutionOrderId, cancellationToken);
        if (executionOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ExecutionOrderNotFound(separationOrder.ExecutionOrderId));

        executionOrder.MarkReadyToStart();

        await separationOrderRepository.SaveChangesAsync(cancellationToken);

        return SeparationOrderMapper.ToDto(separationOrder);
    }
}
