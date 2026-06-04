namespace GarageFlow.Api.Services.DTOs;

public sealed record UpdateServiceRequest(
    string Name,
    string? Description,
    decimal BasePrice,
    int? EstimatedDurationMinutes);
