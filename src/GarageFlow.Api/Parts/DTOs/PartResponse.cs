namespace GarageFlow.Api.Parts.DTOs;

public sealed record PartResponse(
    Guid Id,
    string Name,
    string Code,
    string Sku,
    string UnitOfMeasure,
    decimal UnitPrice,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
