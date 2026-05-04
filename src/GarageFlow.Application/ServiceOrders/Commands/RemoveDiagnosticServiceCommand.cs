namespace GarageFlow.Application.ServiceOrders.Commands;

public sealed record RemoveDiagnosticServiceCommand(Guid ServiceOrderId, Guid ServiceId);
