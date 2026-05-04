namespace GarageFlow.Application.Services.Commands;

public sealed record AddServiceSupplyCommand(
    Guid ServiceId,
    Guid SupplyId,
    decimal Quantity);
