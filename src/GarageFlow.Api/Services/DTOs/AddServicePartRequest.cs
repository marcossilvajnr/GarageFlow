namespace GarageFlow.Api.Services.DTOs;

public sealed record AddServicePartRequest(
    Guid PartId,
    int Quantity);
