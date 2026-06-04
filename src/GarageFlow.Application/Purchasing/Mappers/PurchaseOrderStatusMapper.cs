using AppPurchaseOrderStatus = GarageFlow.Application.Purchasing.Enums.PurchaseOrderStatus;
using DomainPurchaseOrderStatus = GarageFlow.Domain.Purchasing.PurchaseOrderStatus;

namespace GarageFlow.Application.Purchasing.Mappers;

internal static class PurchaseOrderStatusMapper
{
    internal static DomainPurchaseOrderStatus ToDomain(AppPurchaseOrderStatus status) =>
        status switch
        {
            AppPurchaseOrderStatus.Created => DomainPurchaseOrderStatus.Created,
            AppPurchaseOrderStatus.Started => DomainPurchaseOrderStatus.Started,
            AppPurchaseOrderStatus.Completed => DomainPurchaseOrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    internal static AppPurchaseOrderStatus ToApplication(DomainPurchaseOrderStatus status) =>
        status switch
        {
            DomainPurchaseOrderStatus.Created => AppPurchaseOrderStatus.Created,
            DomainPurchaseOrderStatus.Started => AppPurchaseOrderStatus.Started,
            DomainPurchaseOrderStatus.Completed => AppPurchaseOrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
