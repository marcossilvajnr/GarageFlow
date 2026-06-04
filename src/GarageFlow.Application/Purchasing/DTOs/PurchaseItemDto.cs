using GarageFlow.Application.Purchasing.Enums;

namespace GarageFlow.Application.Purchasing.DTOs;

public sealed record PurchaseItemDto(
    Guid ItemId,
    PurchaseItemType ItemType,
    string ItemName,
    decimal Quantity,
    decimal UnitPrice,
    decimal Subtotal);
