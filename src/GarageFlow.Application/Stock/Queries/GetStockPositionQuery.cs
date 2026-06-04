using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Application.Stock.Queries;

public sealed record GetStockPositionQuery(Guid ItemId, StockItemType ItemType);
