namespace GarageFlow.Api.DTOs.Services;

public sealed record ServicePartResponse(
    Guid PartId,
    string PartName,
    int Quantity);
