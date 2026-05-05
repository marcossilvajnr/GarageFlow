namespace GarageFlow.Application.Stock.DTOs;

public sealed record ListStockOperationsResult(
    IReadOnlyList<StockOperationDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
