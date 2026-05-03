namespace GarageFlow.Api.DTOs.Supplies;

public sealed record UpdateSupplyRequest(
    string Name,
    string UnitOfMeasure,
    decimal BaseCost,
    Guid? PreferredSupplierId);
