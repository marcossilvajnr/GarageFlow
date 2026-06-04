using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Application.Stock.Commands;

public sealed record ReleaseStockReservationCommand(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    string? Reason,
    string? PerformedBy,
    Guid? ReferenceId,
    string? ReferenceType);
