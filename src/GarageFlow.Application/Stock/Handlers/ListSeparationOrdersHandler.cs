using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Queries;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class ListSeparationOrdersHandler(ISeparationOrderRepository separationOrderRepository)
{
    public async Task<PagedSeparationOrderResult> HandleAsync(ListSeparationOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await separationOrderRepository.ListAsync(query.Page, query.PageSize, cancellationToken);

        return new PagedSeparationOrderResult(
            items.Select(SeparationOrderMapper.ToDto).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }
}
