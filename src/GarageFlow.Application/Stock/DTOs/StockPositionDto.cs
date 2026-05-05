using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.DTOs;

public sealed record StockPositionDto(
    Guid StockId,
    Guid ItemId,
    StockItemType ItemType,
    decimal TotalQuantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal MinimumQuantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
