using GarageFlow.Application.Executions.Commands;
using GarageFlow.Application.Executions.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Executions.Handlers;

public sealed class StartExecutionOrderHandler(IExecutionOrderRepository executionOrderRepository)
{
    public async Task<ExecutionOrderDto> HandleAsync(StartExecutionOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.MechanicId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidExecutionOrderMechanicId);

        var executionOrder = await executionOrderRepository.GetByIdAsync(command.ExecutionOrderId, cancellationToken);
        if (executionOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ExecutionOrderNotFound(command.ExecutionOrderId));

        executionOrder.StartExecution(command.MechanicId);

        await executionOrderRepository.SaveChangesAsync(cancellationToken);

        return ExecutionOrderMapper.ToDto(executionOrder);
    }
}
