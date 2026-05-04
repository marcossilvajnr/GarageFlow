namespace GarageFlow.Application.Services.Commands;

public sealed record AddServicePartCommand(
    Guid ServiceId,
    Guid PartId,
    int Quantity);
