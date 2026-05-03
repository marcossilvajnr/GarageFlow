namespace GarageFlow.Application.Services.Commands;

public sealed record CreateServiceCommand(
    string Code,
    string Name,
    string? Description,
    decimal BasePrice,
    int? EstimatedDurationMinutes);
