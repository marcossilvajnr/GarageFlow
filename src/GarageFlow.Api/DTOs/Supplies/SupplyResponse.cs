namespace GarageFlow.Api.DTOs.Supplies;

public sealed record SupplyResponse(
    Guid Id,
    string Name,
    string Code,
    string UnitOfMeasure,
    decimal BaseCost,
    Guid? PreferredSupplierId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
