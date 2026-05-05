using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Commands;

public sealed record CreateStockEntryCommand(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    decimal MinimumQuantity,
    string? Reason,
    Guid? ReferenceId);
