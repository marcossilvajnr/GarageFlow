using GarageFlow.Domain.Stock;

namespace GarageFlow.Api.DTOs.Stock;

public sealed record StockPositionResponse(
    Guid StockId,
    Guid ItemId,
    StockItemType ItemType,
    decimal TotalQuantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal MinimumQuantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
