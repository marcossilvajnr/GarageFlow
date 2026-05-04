namespace GarageFlow.Application.ServiceOrders.Commands;

public sealed record CompleteDiagnosticCommand(Guid ServiceOrderId, string Description);
