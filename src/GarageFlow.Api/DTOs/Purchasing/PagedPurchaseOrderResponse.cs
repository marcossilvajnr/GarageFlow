namespace GarageFlow.Api.DTOs.Purchasing;

public sealed record PagedPurchaseOrderResponse(
    IReadOnlyList<PurchaseOrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
