namespace GarageFlow.Api.DTOs.Executions;

public sealed record CreateExecutionOrderRequest(Guid ServiceOrderId, Guid ServiceId, Guid MechanicId);
