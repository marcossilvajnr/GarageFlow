namespace GarageFlow.Application.ServiceOrders.Commands;

public sealed record AddDiagnosticServiceCommand(Guid ServiceOrderId, Guid ServiceId);
