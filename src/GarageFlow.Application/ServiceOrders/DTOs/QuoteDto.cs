using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record QuoteDto(
    Guid Id,
    Guid ServiceOrderId,
    IReadOnlyList<QuoteItemDto> Items,
    decimal TotalAmount,
    QuoteStatus Status,
    DateTime GeneratedAt,
    DateTime? AcceptedAt,
    DateTime? RejectedAt,
    string? RejectionReason);
