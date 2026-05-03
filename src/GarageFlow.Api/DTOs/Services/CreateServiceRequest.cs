namespace GarageFlow.Api.DTOs.Services;

public sealed record CreateServiceRequest(
    string Code,
    string Name,
    string? Description,
    decimal BasePrice,
    int? EstimatedDurationMinutes);
