namespace GarageFlow.Api.Supplies.DTOs;

public sealed record CreateSupplyRequest(
    string Name,
    string Code,
    string UnitOfMeasure,
    decimal BaseCost,
    Guid? PreferredSupplierId);
