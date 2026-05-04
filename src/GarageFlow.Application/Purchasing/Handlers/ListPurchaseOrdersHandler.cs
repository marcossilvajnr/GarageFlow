using GarageFlow.Application.Purchasing.DTOs;
using GarageFlow.Application.Purchasing.Queries;
using GarageFlow.Domain.Purchasing;

namespace GarageFlow.Application.Purchasing.Handlers;

public sealed class ListPurchaseOrdersHandler(IPurchaseOrderRepository purchaseOrderRepository)
{
    public async Task<PagedPurchaseOrderResult> HandleAsync(
        ListPurchaseOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await purchaseOrderRepository.ListAsync(query.Page, query.PageSize, cancellationToken);

        return new PagedPurchaseOrderResult(
            items.Select(PurchaseOrderMapper.ToDto).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }
}
