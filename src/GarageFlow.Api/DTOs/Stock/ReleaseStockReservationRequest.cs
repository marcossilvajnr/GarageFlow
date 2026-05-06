using GarageFlow.Domain.Stock;

namespace GarageFlow.Api.DTOs.Stock;

public sealed record ReleaseStockReservationRequest(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    string? Reason,
    string? PerformedBy,
    Guid? ReferenceId,
    string? ReferenceType);
