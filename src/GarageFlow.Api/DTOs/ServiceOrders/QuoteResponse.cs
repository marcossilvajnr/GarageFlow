using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Api.DTOs.ServiceOrders;

public sealed record QuoteResponse(
    Guid Id,
    Guid ServiceOrderId,
    IReadOnlyList<QuoteItemResponse> Items,
    decimal TotalAmount,
    QuoteStatus Status,
    DateTime GeneratedAt,
    DateTime? AcceptedAt,
    DateTime? RejectedAt,
    string? RejectionReason);
