using GarageFlow.Domain.Purchasing;

namespace GarageFlow.Api.DTOs.Purchasing;

public sealed record PurchaseItemResponse(
    Guid ItemId,
    PurchaseItemType ItemType,
    string ItemName,
    decimal Quantity,
    decimal UnitPrice,
    decimal Subtotal);
