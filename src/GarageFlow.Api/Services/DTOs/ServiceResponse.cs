namespace GarageFlow.Api.Services.DTOs;

public sealed record ServiceResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    decimal BasePrice,
    int? EstimatedDurationMinutes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ServicePartResponse> Parts,
    IReadOnlyList<ServiceSupplyResponse> Supplies);
