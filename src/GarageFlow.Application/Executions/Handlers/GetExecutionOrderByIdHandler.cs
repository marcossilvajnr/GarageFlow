using GarageFlow.Application.Executions.DTOs;
using GarageFlow.Application.Executions.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Executions.Handlers;

public sealed class GetExecutionOrderByIdHandler(IExecutionOrderRepository executionOrderRepository)
{
    public async Task<ExecutionOrderDto> HandleAsync(GetExecutionOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var executionOrder = await executionOrderRepository.GetByIdAsync(query.Id, cancellationToken);
        if (executionOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ExecutionOrderNotFound(query.Id));

        return ExecutionOrderMapper.ToDto(executionOrder);
    }
}
