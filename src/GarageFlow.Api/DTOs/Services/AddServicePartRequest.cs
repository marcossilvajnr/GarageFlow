namespace GarageFlow.Api.DTOs.Services;

public sealed record AddServicePartRequest(
    Guid PartId,
    int Quantity);
