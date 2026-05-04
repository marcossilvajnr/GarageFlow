namespace GarageFlow.Application.ServiceOrders.Commands;

public sealed record RejectQuoteCommand(Guid ServiceOrderId, string Reason);
