using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record ServiceOrderDto(
    Guid Id,
    Guid CustomerId,
    Guid VehicleId,
    ServiceOrderStatus Status,
    DiagnosticDto? Diagnostic,
    QuoteDto? Quote,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ServiceOrderServiceItemDto> Services,
    IReadOnlyList<ServiceOrderServiceHistoryDto> ServiceHistory);
