namespace GarageFlow.Application.Services.Commands;

public sealed record UpdateServiceCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal BasePrice,
    int? EstimatedDurationMinutes);
