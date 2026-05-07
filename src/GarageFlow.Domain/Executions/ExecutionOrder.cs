using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Executions;

public sealed class ExecutionOrder
{
    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid? MechanicId { get; private set; }
    public ExecutionOrderStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public decimal? ActualTimeMinutes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ExecutionOrder() { }

    public static ExecutionOrder Create(Guid serviceOrderId, Guid serviceId, Guid mechanicId)
    {
        if (serviceOrderId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidExecutionOrderServiceOrderId);

        if (serviceId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidExecutionOrderServiceId);

        if (mechanicId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidExecutionOrderMechanicId);

        return new ExecutionOrder
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = serviceOrderId,
            ServiceId = serviceId,
            MechanicId = mechanicId,
            Status = ExecutionOrderStatus.Pending,
            StartedAt = null,
            CompletedAt = null,
            ActualTimeMinutes = null,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkReadyToStart()
    {
        if (Status == ExecutionOrderStatus.Pending)
            Status = ExecutionOrderStatus.Ready;

        // Idempotent: if already Ready, InExecution, or Completed — no-op, no error
    }

    public void StartExecution()
    {
        if (Status != ExecutionOrderStatus.Ready)
            throw new InvalidExecutionOrderStatusTransitionException(
                DomainErrorMessages.ExecutionOrderNotReady);

        if (MechanicId is null || MechanicId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidExecutionOrderMechanicId);

        Status = ExecutionOrderStatus.InExecution;
        StartedAt = DateTime.UtcNow;
    }

    public void CompleteExecution()
    {
        if (Status != ExecutionOrderStatus.InExecution)
            throw new InvalidExecutionOrderStatusTransitionException(
                DomainErrorMessages.ExecutionOrderNotInExecution);

        if (!StartedAt.HasValue)
            throw new InvalidExecutionOrderStatusTransitionException(
                DomainErrorMessages.ExecutionOrderNotInExecution);

        var completedAt = DateTime.UtcNow;
        CompletedAt = completedAt;
        Status = ExecutionOrderStatus.Completed;
        ActualTimeMinutes = (decimal)(completedAt - StartedAt.Value).TotalMinutes;
    }
}
