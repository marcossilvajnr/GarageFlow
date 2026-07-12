using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class ListOperationalServiceOrdersHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task<PagedServiceOrderResult> HandleAsync(ListOperationalServiceOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await serviceOrderRepository.ListOperationalAsync(query.Page, query.PageSize, cancellationToken);

        return new PagedServiceOrderResult(
            items.Select(ServiceOrderMapper.ToDto).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }
}
