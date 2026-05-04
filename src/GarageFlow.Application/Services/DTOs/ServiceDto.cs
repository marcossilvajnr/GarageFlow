namespace GarageFlow.Application.Services.DTOs;

public sealed record ServiceDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    decimal BasePrice,
    int? EstimatedDurationMinutes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ServicePartItemDto> Parts,
    IReadOnlyList<ServiceSupplyItemDto> Supplies);
