using GarageFlow.Application.Executions.DTOs;
using GarageFlow.Application.Executions.Queries;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Application.Executions.Handlers;

public sealed class ListExecutionOrdersHandler(IExecutionOrderRepository executionOrderRepository)
{
    public async Task<PagedExecutionOrderResult> HandleAsync(ListExecutionOrdersQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Page <= 0 || query.PageSize <= 0 || query.PageSize > ExecutionOrderPaginationDefaults.MaxPageSize)
            throw new DomainException(DomainErrorMessages.InvalidPaginationParameters);

        var (items, totalCount) = await executionOrderRepository.ListAsync(query.Page, query.PageSize, cancellationToken);

        return new PagedExecutionOrderResult(
            items.Select(ExecutionOrderMapper.ToDto).ToList(),
            totalCount,
            query.Page,
            query.PageSize);
    }
}
