using AppStockOperationType = GarageFlow.Application.Stock.Enums.StockOperationType;
using DomainStockOperationType = GarageFlow.Domain.Stock.StockOperationType;

namespace GarageFlow.Application.Stock.Mappers;

internal static class StockOperationTypeMapper
{
    internal static DomainStockOperationType ToDomain(AppStockOperationType operationType) =>
        operationType switch
        {
            AppStockOperationType.Entry => DomainStockOperationType.Entry,
            AppStockOperationType.Reserve => DomainStockOperationType.Reserve,
            AppStockOperationType.Release => DomainStockOperationType.Release,
            AppStockOperationType.Consume => DomainStockOperationType.Consume,
            AppStockOperationType.Adjust => DomainStockOperationType.Adjust,
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };

    internal static AppStockOperationType ToApplication(DomainStockOperationType operationType) =>
        operationType switch
        {
            DomainStockOperationType.Entry => AppStockOperationType.Entry,
            DomainStockOperationType.Reserve => AppStockOperationType.Reserve,
            DomainStockOperationType.Release => AppStockOperationType.Release,
            DomainStockOperationType.Consume => AppStockOperationType.Consume,
            DomainStockOperationType.Adjust => AppStockOperationType.Adjust,
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };
}
