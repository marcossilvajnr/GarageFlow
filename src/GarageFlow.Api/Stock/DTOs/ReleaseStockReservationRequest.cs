using GarageFlow.Domain.Stock;

namespace GarageFlow.Api.Stock.DTOs;

public sealed record ReleaseStockReservationRequest(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    string? Reason,
    string? PerformedBy,
    Guid? ReferenceId,
    string? ReferenceType);
