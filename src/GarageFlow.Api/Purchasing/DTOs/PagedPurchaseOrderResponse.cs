namespace GarageFlow.Api.Purchasing.DTOs;

public sealed record PagedPurchaseOrderResponse(
    IReadOnlyList<PurchaseOrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
