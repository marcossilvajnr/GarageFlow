namespace GarageFlow.Api.DTOs.Stock;

public sealed record PagedStockOperationsResponse(
    IReadOnlyList<StockOperationResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
