namespace GarageFlow.Api.Services.DTOs;

public sealed record CreateServiceRequest(
    string Code,
    string Name,
    string? Description,
    decimal BasePrice,
    int? EstimatedDurationMinutes);
