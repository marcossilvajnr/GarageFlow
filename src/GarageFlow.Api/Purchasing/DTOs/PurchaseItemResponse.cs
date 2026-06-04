using GarageFlow.Application.Purchasing.Enums;

namespace GarageFlow.Api.Purchasing.DTOs;

public sealed record PurchaseItemResponse(
    Guid ItemId,
    PurchaseItemType ItemType,
    string ItemName,
    decimal Quantity,
    decimal UnitPrice,
    decimal Subtotal);
