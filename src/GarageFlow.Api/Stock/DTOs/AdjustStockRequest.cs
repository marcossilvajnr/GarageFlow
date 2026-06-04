using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Api.Stock.DTOs;

public sealed record AdjustStockRequest(
    Guid ItemId,
    StockItemType ItemType,
    decimal QuantityDelta,
    string Reason,
    Guid? ReferenceId);
