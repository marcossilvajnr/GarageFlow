using GarageFlow.Domain.Executions;

namespace GarageFlow.Api.Executions.DTOs;

public sealed record ExecutionOrderResponse(
    Guid Id,
    Guid ServiceOrderId,
    Guid ServiceId,
    Guid? MechanicId,
    ExecutionOrderStatus Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    decimal? ActualTimeMinutes,
    DateTime CreatedAt);
