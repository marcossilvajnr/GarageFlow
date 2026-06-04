namespace GarageFlow.Api.Supplies.DTOs;

public sealed record UpdateSupplyRequest(
    string Name,
    string UnitOfMeasure,
    decimal BaseCost,
    Guid? PreferredSupplierId);
