using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Api.ServiceOrders.DTOs;

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
