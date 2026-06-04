using AppDiagnosticStatus = GarageFlow.Application.ServiceOrders.Enums.DiagnosticStatus;
using DomainDiagnosticStatus = GarageFlow.Domain.ServiceOrders.DiagnosticStatus;

namespace GarageFlow.Application.ServiceOrders.Mappers;

internal static class DiagnosticStatusMapper
{
    internal static DomainDiagnosticStatus ToDomain(AppDiagnosticStatus status) =>
        status switch
        {
            AppDiagnosticStatus.InProgress => DomainDiagnosticStatus.InProgress,
            AppDiagnosticStatus.Completed => DomainDiagnosticStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    internal static AppDiagnosticStatus ToApplication(DomainDiagnosticStatus status) =>
        status switch
        {
            DomainDiagnosticStatus.InProgress => AppDiagnosticStatus.InProgress,
            DomainDiagnosticStatus.Completed => AppDiagnosticStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
