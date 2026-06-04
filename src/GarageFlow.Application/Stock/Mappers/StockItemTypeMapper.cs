using AppStockItemType = GarageFlow.Application.Stock.Enums.StockItemType;
using DomainStockItemType = GarageFlow.Domain.Stock.StockItemType;

namespace GarageFlow.Application.Stock.Mappers;

internal static class StockItemTypeMapper
{
    internal static DomainStockItemType ToDomain(AppStockItemType itemType) =>
        itemType switch
        {
            AppStockItemType.Part => DomainStockItemType.Part,
            AppStockItemType.Supply => DomainStockItemType.Supply,
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
        };

    internal static AppStockItemType ToApplication(DomainStockItemType itemType) =>
        itemType switch
        {
            DomainStockItemType.Part => AppStockItemType.Part,
            DomainStockItemType.Supply => AppStockItemType.Supply,
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
        };
}
