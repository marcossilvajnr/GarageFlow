namespace GarageFlow.Application.Executions.Commands;

public sealed record CreateExecutionOrderCommand(Guid ServiceOrderId, Guid ServiceId, Guid MechanicId);
