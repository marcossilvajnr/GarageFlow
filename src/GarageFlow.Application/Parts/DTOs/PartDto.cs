namespace GarageFlow.Application.Parts.DTOs;

public sealed record PartDto(
    Guid Id,
    string Name,
    string Code,
    string Sku,
    string UnitOfMeasure,
    decimal UnitPrice,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
