using GarageFlow.Domain.Stock;

namespace GarageFlow.Api.Stock.DTOs;

public sealed record ReserveStockRequest(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    string? Reason,
    Guid? ReferenceId);
