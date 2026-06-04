using AppServiceOrderStatus = GarageFlow.Application.ServiceOrders.Enums.ServiceOrderStatus;
using DomainServiceOrderStatus = GarageFlow.Domain.ServiceOrders.ServiceOrderStatus;

namespace GarageFlow.Application.ServiceOrders.Mappers;

internal static class ServiceOrderStatusMapper
{
    internal static DomainServiceOrderStatus ToDomain(AppServiceOrderStatus status) =>
        status switch
        {
            AppServiceOrderStatus.Received => DomainServiceOrderStatus.Received,
            AppServiceOrderStatus.InDiagnostic => DomainServiceOrderStatus.InDiagnostic,
            AppServiceOrderStatus.WaitingApproval => DomainServiceOrderStatus.WaitingApproval,
            AppServiceOrderStatus.InExecution => DomainServiceOrderStatus.InExecution,
            AppServiceOrderStatus.Finished => DomainServiceOrderStatus.Finished,
            AppServiceOrderStatus.Delivered => DomainServiceOrderStatus.Delivered,
            AppServiceOrderStatus.Approved => DomainServiceOrderStatus.Approved,
            AppServiceOrderStatus.Rejected => DomainServiceOrderStatus.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    internal static AppServiceOrderStatus ToApplication(DomainServiceOrderStatus status) =>
        status switch
        {
            DomainServiceOrderStatus.Received => AppServiceOrderStatus.Received,
            DomainServiceOrderStatus.InDiagnostic => AppServiceOrderStatus.InDiagnostic,
            DomainServiceOrderStatus.WaitingApproval => AppServiceOrderStatus.WaitingApproval,
            DomainServiceOrderStatus.InExecution => AppServiceOrderStatus.InExecution,
            DomainServiceOrderStatus.Finished => AppServiceOrderStatus.Finished,
            DomainServiceOrderStatus.Delivered => AppServiceOrderStatus.Delivered,
            DomainServiceOrderStatus.Approved => AppServiceOrderStatus.Approved,
            DomainServiceOrderStatus.Rejected => AppServiceOrderStatus.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
