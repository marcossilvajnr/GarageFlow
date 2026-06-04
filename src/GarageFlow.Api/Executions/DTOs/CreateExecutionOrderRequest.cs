namespace GarageFlow.Api.Executions.DTOs;

public sealed record CreateExecutionOrderRequest(Guid ServiceOrderId, Guid ServiceId, Guid MechanicId);
