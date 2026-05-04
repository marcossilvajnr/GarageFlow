namespace GarageFlow.Application.ServiceOrders.Commands;

public sealed record StartDiagnosticCommand(Guid ServiceOrderId, Guid MechanicId);
