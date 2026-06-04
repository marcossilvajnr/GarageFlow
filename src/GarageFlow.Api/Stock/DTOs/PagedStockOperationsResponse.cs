namespace GarageFlow.Api.Stock.DTOs;

public sealed record PagedStockOperationsResponse(
    IReadOnlyList<StockOperationResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
