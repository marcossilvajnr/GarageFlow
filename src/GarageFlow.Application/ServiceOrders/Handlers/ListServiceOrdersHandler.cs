using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class ListServiceOrdersHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task<PagedServiceOrderResult> HandleAsync(ListServiceOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await serviceOrderRepository.ListAsync(query.Page, query.PageSize, cancellationToken);

        return new PagedServiceOrderResult(
            items.Select(ServiceOrderMapper.ToDto).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }
}
