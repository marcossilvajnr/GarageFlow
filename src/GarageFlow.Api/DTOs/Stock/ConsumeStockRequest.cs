using GarageFlow.Domain.Stock;

namespace GarageFlow.Api.DTOs.Stock;

public sealed record ConsumeStockRequest(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    string? Reason,
    Guid? ReferenceId);
