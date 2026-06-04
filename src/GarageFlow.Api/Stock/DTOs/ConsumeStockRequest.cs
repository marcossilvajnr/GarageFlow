using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Api.Stock.DTOs;

public sealed record ConsumeStockRequest(
    Guid ItemId,
    StockItemType ItemType,
    decimal Quantity,
    string? Reason,
    Guid? ReferenceId);
