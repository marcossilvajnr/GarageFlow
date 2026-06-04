using GarageFlow.Application.Purchasing.Enums;

namespace GarageFlow.Application.Purchasing.Commands;

public sealed record CreatePurchaseItemCommand(
    Guid ItemId,
    PurchaseItemType ItemType,
    string ItemName,
    decimal Quantity,
    decimal UnitPrice);
