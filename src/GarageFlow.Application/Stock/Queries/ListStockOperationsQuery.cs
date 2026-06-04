using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Application.Stock.Queries;

public sealed record ListStockOperationsQuery(
    Guid ItemId,
    StockItemType ItemType,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize);
