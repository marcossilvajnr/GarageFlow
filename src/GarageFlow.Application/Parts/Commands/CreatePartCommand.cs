namespace GarageFlow.Application.Parts.Commands;

public sealed record CreatePartCommand(
    string Name,
    string Code,
    string Sku,
    string UnitOfMeasure,
    decimal UnitPrice);
