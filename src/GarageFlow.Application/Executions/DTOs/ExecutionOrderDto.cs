using GarageFlow.Domain.Executions;

namespace GarageFlow.Application.Executions.DTOs;

public sealed record ExecutionOrderDto(
    Guid Id,
    Guid ServiceOrderId,
    Guid ServiceId,
    Guid? MechanicId,
    ExecutionOrderStatus Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    decimal? ActualTimeMinutes,
    DateTime CreatedAt);
