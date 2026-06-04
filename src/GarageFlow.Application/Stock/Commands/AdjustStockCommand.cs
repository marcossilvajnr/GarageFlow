using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Application.Stock.Commands;

public sealed record AdjustStockCommand(
    Guid ItemId,
    StockItemType ItemType,
    decimal QuantityDelta,
    string Reason,
    Guid? ReferenceId);
