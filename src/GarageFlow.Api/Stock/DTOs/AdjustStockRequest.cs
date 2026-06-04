using GarageFlow.Domain.Stock;

namespace GarageFlow.Api.Stock.DTOs;

public sealed record AdjustStockRequest(
    Guid ItemId,
    StockItemType ItemType,
    decimal QuantityDelta,
    string Reason,
    Guid? ReferenceId);
