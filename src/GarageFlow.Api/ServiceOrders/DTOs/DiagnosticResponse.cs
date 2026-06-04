using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record DiagnosticResponse(
    Guid Id,
    Guid MechanicId,
    string? Description,
    IReadOnlyList<Guid> SelectedServices,
    DateTime StartedAt,
    DateTime? CompletedAt,
    DiagnosticStatus Status);
