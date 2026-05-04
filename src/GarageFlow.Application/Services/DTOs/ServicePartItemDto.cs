namespace GarageFlow.Application.Services.DTOs;

public sealed record ServicePartItemDto(
    Guid PartId,
    string PartName,
    int Quantity);
