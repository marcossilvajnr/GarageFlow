using GarageFlow.Application.Executions.Commands;
using GarageFlow.Application.Executions.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Executions.Handlers;

public sealed class CreateExecutionOrderHandler(IExecutionOrderRepository executionOrderRepository)
{
    public async Task<ExecutionOrderDto> HandleAsync(CreateExecutionOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ServiceOrderId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidExecutionOrderServiceOrderId);

        if (command.ServiceId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidExecutionOrderServiceId);

        var executionOrder = ExecutionOrder.Create(command.ServiceOrderId, command.ServiceId);

        await executionOrderRepository.AddAsync(executionOrder, cancellationToken);
        await executionOrderRepository.SaveChangesAsync(cancellationToken);

        return ExecutionOrderMapper.ToDto(executionOrder);
    }
}
