using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Commands;

public sealed record ReserveStockCommand(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    string? Reason,
    Guid? ReferenceId);
