using AppExecutionOrderStatus = GarageFlow.Application.Executions.Enums.ExecutionOrderStatus;
using DomainExecutionOrderStatus = GarageFlow.Domain.Executions.ExecutionOrderStatus;

namespace GarageFlow.Application.Executions.Mappers;

internal static class ExecutionOrderStatusMapper
{
    internal static DomainExecutionOrderStatus ToDomain(AppExecutionOrderStatus status) =>
        status switch
        {
            AppExecutionOrderStatus.Pending => DomainExecutionOrderStatus.Pending,
            AppExecutionOrderStatus.Ready => DomainExecutionOrderStatus.Ready,
            AppExecutionOrderStatus.InExecution => DomainExecutionOrderStatus.InExecution,
            AppExecutionOrderStatus.Completed => DomainExecutionOrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    internal static AppExecutionOrderStatus ToApplication(DomainExecutionOrderStatus status) =>
        status switch
        {
            DomainExecutionOrderStatus.Pending => AppExecutionOrderStatus.Pending,
            DomainExecutionOrderStatus.Ready => AppExecutionOrderStatus.Ready,
            DomainExecutionOrderStatus.InExecution => AppExecutionOrderStatus.InExecution,
            DomainExecutionOrderStatus.Completed => AppExecutionOrderStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
