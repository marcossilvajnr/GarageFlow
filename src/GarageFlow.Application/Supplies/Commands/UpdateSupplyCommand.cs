namespace GarageFlow.Application.Supplies.Commands;

public sealed record UpdateSupplyCommand(
    Guid Id,
    string Name,
    string UnitOfMeasure,
    decimal BaseCost,
    Guid? PreferredSupplierId);
