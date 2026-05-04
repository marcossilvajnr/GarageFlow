namespace GarageFlow.Application.Services.Commands;

public sealed record RemoveServicePartCommand(
    Guid ServiceId,
    Guid PartId);
