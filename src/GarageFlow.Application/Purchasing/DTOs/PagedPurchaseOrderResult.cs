namespace GarageFlow.Application.Purchasing.DTOs;

public sealed record PagedPurchaseOrderResult(
    IReadOnlyList<PurchaseOrderDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
