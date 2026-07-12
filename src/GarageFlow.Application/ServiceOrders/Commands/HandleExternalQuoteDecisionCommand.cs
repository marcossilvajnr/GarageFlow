namespace GarageFlow.Application.ServiceOrders.Commands;

public sealed record HandleExternalQuoteDecisionCommand(
    Guid ServiceOrderId,
    string? Decision,
    string? Reason,
    string? ExternalNotificationId,
    string Source);
