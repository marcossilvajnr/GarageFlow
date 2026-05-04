using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Purchasing;

public sealed class PurchaseItem
{
    public Guid ItemId { get; private set; }
    public PurchaseItemType ItemType { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => UnitPrice * Quantity;

    private PurchaseItem() { }

    public static PurchaseItem Create(
        Guid itemId,
        PurchaseItemType itemType,
        string itemName,
        decimal quantity,
        decimal unitPrice)
    {
        if (itemId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidPurchaseItemId);

        if (!Enum.IsDefined(itemType))
            throw new DomainException(DomainErrorMessages.InvalidPurchaseItemType);

        if (string.IsNullOrWhiteSpace(itemName))
            throw new DomainException(DomainErrorMessages.InvalidPurchaseItemName);

        if (quantity <= 0)
            throw new DomainException(DomainErrorMessages.InvalidPurchaseItemQuantity);

        if (unitPrice < 0)
            throw new DomainException(DomainErrorMessages.InvalidPurchaseItemUnitPrice);

        return new PurchaseItem
        {
            ItemId = itemId,
            ItemType = itemType,
            ItemName = itemName.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
