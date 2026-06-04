using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record ServiceOrderResponse(
    Guid Id,
    Guid CustomerId,
    Guid VehicleId,
    Guid FrontDeskEmployeeId,
    ServiceOrderStatus Status,
    DiagnosticResponse? Diagnostic,
    QuoteResponse? Quote,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ServiceOrderServiceResponse> Services,
    IReadOnlyList<ServiceOrderServiceHistoryResponse> ServiceHistory);
