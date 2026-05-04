using GarageFlow.Application.Executions.Commands;
using GarageFlow.Application.Executions.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Executions.Handlers;

public sealed class MarkExecutionOrderReadyHandler(IExecutionOrderRepository executionOrderRepository)
{
    public async Task<ExecutionOrderDto> HandleAsync(MarkExecutionOrderReadyCommand command, CancellationToken cancellationToken = default)
    {
        var executionOrder = await executionOrderRepository.GetByIdAsync(command.ExecutionOrderId, cancellationToken);
        if (executionOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ExecutionOrderNotFound(command.ExecutionOrderId));

        executionOrder.MarkReadyToStart();

        await executionOrderRepository.SaveChangesAsync(cancellationToken);

        return ExecutionOrderMapper.ToDto(executionOrder);
    }
}
