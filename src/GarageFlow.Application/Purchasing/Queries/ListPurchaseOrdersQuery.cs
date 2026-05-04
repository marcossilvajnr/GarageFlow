namespace GarageFlow.Application.Purchasing.Queries;

public sealed record ListPurchaseOrdersQuery(
    int Page = PurchaseOrderPaginationDefaults.DefaultPage,
    int PageSize = PurchaseOrderPaginationDefaults.DefaultPageSize);
