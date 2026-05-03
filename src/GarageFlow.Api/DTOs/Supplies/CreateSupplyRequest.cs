namespace GarageFlow.Api.DTOs.Supplies;

public sealed record CreateSupplyRequest(
    string Name,
    string Code,
    string UnitOfMeasure,
    decimal BaseCost,
    Guid? PreferredSupplierId);
