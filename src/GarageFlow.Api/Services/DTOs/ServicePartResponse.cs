namespace GarageFlow.Api.Services.DTOs;

public sealed record ServicePartResponse(
    Guid PartId,
    string PartName,
    int Quantity);
