using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Application.ServiceOrders.Mappers;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class ListOperationalServiceOrdersHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task<PagedOperationalServiceOrderResult> HandleAsync(ListOperationalServiceOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await serviceOrderRepository.ListOperationalAsync(query.Page, query.PageSize, cancellationToken);

        return new PagedOperationalServiceOrderResult(
            items.Select(ToOperationalDto).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    private static OperationalServiceOrderDto ToOperationalDto(ServiceOrder serviceOrder)
    {
        var status = ServiceOrderStatusMapper.ToApplication(serviceOrder.Status);
        return new OperationalServiceOrderDto(
            serviceOrder.Id,
            status,
            ServiceOrderStatusLabelMapper.ToLabel(status));
    }
}
