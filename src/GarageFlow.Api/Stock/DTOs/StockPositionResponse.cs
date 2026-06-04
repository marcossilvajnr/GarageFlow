using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Api.Stock.DTOs;

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
