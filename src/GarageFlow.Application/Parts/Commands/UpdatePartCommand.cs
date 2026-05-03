namespace GarageFlow.Application.Parts.Commands;

public sealed record UpdatePartCommand(Guid Id, string Name, string UnitOfMeasure, decimal UnitPrice);
