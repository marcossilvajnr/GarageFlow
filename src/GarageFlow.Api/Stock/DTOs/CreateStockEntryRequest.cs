using GarageFlow.Domain.Stock;

namespace GarageFlow.Api.Stock.DTOs;

public sealed record CreateStockEntryRequest(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    decimal MinimumQuantity,
    string? Reason,
    Guid? ReferenceId);
