using AppServiceOrderServiceAction = GarageFlow.Application.ServiceOrders.Enums.ServiceOrderServiceAction;
using DomainServiceOrderServiceAction = GarageFlow.Domain.ServiceOrders.ServiceOrderServiceAction;

namespace GarageFlow.Application.ServiceOrders.Mappers;

internal static class ServiceOrderServiceActionMapper
{
    internal static DomainServiceOrderServiceAction ToDomain(AppServiceOrderServiceAction action) =>
        action switch
        {
            AppServiceOrderServiceAction.Added => DomainServiceOrderServiceAction.Added,
            AppServiceOrderServiceAction.Removed => DomainServiceOrderServiceAction.Removed,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };

    internal static AppServiceOrderServiceAction ToApplication(DomainServiceOrderServiceAction action) =>
        action switch
        {
            DomainServiceOrderServiceAction.Added => AppServiceOrderServiceAction.Added,
            DomainServiceOrderServiceAction.Removed => AppServiceOrderServiceAction.Removed,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
}
