using System.Text.Json;

namespace GarageFlow.Api.ServiceOrders.External;

public sealed record ExternalQuoteDecisionNotificationRequest(
    Guid ServiceOrderId,
    JsonElement Decision,
    string? Reason,
    string? ExternalNotificationId,
    string Source);
