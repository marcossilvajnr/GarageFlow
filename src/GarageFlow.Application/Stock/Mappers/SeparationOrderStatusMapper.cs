using AppSeparationOrderStatus = GarageFlow.Application.Stock.Enums.SeparationOrderStatus;
using DomainSeparationOrderStatus = GarageFlow.Domain.Stock.SeparationOrderStatus;

namespace GarageFlow.Application.Stock.Mappers;

internal static class SeparationOrderStatusMapper
{
    internal static DomainSeparationOrderStatus ToDomain(AppSeparationOrderStatus status) =>
        status switch
        {
            AppSeparationOrderStatus.Pending => DomainSeparationOrderStatus.Pending,
            AppSeparationOrderStatus.WaitingPurchase => DomainSeparationOrderStatus.WaitingPurchase,
            AppSeparationOrderStatus.WaitingPickup => DomainSeparationOrderStatus.WaitingPickup,
            AppSeparationOrderStatus.Separated => DomainSeparationOrderStatus.Separated,
            AppSeparationOrderStatus.Completed => DomainSeparationOrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    internal static AppSeparationOrderStatus ToApplication(DomainSeparationOrderStatus status) =>
        status switch
        {
            DomainSeparationOrderStatus.Pending => AppSeparationOrderStatus.Pending,
            DomainSeparationOrderStatus.WaitingPurchase => AppSeparationOrderStatus.WaitingPurchase,
            DomainSeparationOrderStatus.WaitingPickup => AppSeparationOrderStatus.WaitingPickup,
            DomainSeparationOrderStatus.Separated => AppSeparationOrderStatus.Separated,
            DomainSeparationOrderStatus.Completed => AppSeparationOrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
