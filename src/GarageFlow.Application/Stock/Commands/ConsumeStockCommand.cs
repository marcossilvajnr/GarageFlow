using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Application.Stock.Commands;

public sealed record ConsumeStockCommand(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    string? Reason,
    Guid? ReferenceId);
