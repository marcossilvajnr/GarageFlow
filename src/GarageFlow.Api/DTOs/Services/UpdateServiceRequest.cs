namespace GarageFlow.Api.DTOs.Services;

public sealed record UpdateServiceRequest(
    string Name,
    string? Description,
    decimal BasePrice,
    int? EstimatedDurationMinutes);
