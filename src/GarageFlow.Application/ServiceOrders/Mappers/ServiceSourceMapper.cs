using AppServiceSource = GarageFlow.Application.ServiceOrders.Enums.ServiceSource;
using DomainServiceSource = GarageFlow.Domain.ServiceOrders.ServiceSource;

namespace GarageFlow.Application.ServiceOrders.Mappers;

internal static class ServiceSourceMapper
{
    internal static DomainServiceSource ToDomain(AppServiceSource source) =>
        source switch
        {
            AppServiceSource.FrontDesk => DomainServiceSource.FrontDesk,
            AppServiceSource.Diagnostic => DomainServiceSource.Diagnostic,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };

    internal static AppServiceSource ToApplication(DomainServiceSource source) =>
        source switch
        {
            DomainServiceSource.FrontDesk => AppServiceSource.FrontDesk,
            DomainServiceSource.Diagnostic => AppServiceSource.Diagnostic,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
}
