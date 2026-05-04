namespace GarageFlow.Application.Services.Commands;

public sealed record RemoveServiceSupplyCommand(
    Guid ServiceId,
    Guid SupplyId);
