using AppPurchaseItemType = GarageFlow.Application.Purchasing.Enums.PurchaseItemType;
using DomainPurchaseItemType = GarageFlow.Domain.Purchasing.PurchaseItemType;

namespace GarageFlow.Application.Purchasing.Mappers;

internal static class PurchaseItemTypeMapper
{
    internal static DomainPurchaseItemType ToDomain(AppPurchaseItemType itemType) =>
        itemType switch
        {
            AppPurchaseItemType.Part => DomainPurchaseItemType.Part,
            AppPurchaseItemType.Supply => DomainPurchaseItemType.Supply,
            _ => (DomainPurchaseItemType)itemType
        };

    internal static AppPurchaseItemType ToApplication(DomainPurchaseItemType itemType) =>
        itemType switch
        {
            DomainPurchaseItemType.Part => AppPurchaseItemType.Part,
            DomainPurchaseItemType.Supply => AppPurchaseItemType.Supply,
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
        };
}
