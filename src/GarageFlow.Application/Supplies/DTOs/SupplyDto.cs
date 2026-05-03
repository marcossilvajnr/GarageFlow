namespace GarageFlow.Application.Supplies.DTOs;

public sealed record SupplyDto(
    Guid Id,
    string Name,
    string Code,
    string UnitOfMeasure,
    decimal BaseCost,
    Guid? PreferredSupplierId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
