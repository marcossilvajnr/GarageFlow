namespace GarageFlow.Application.Supplies.Commands;

public sealed record CreateSupplyCommand(
    string Name,
    string Code,
    string UnitOfMeasure,
    decimal BaseCost,
    Guid? PreferredSupplierId);
