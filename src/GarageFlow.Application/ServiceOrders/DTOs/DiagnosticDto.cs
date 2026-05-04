using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record DiagnosticDto(
    Guid Id,
    Guid MechanicId,
    string? Description,
    IReadOnlyList<Guid> SelectedServices,
    DateTime StartedAt,
    DateTime? CompletedAt,
    DiagnosticStatus Status);
