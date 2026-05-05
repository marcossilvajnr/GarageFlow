using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Queries;

public sealed record GetStockPositionQuery(Guid ItemId, StockItemType ItemType);
