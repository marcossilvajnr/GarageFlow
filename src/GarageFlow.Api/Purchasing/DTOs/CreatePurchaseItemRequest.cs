using GarageFlow.Domain.Purchasing;

namespace GarageFlow.Api.Purchasing.DTOs;

public sealed record CreatePurchaseItemRequest(
    Guid ItemId,
    PurchaseItemType ItemType,
    string ItemName,
    decimal Quantity,
    decimal UnitPrice);
