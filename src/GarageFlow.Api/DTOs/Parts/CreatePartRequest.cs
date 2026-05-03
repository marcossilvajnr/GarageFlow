namespace GarageFlow.Api.DTOs.Parts;

public sealed record CreatePartRequest(
    string Name,
    string Code,
    string Sku,
    string UnitOfMeasure,
    decimal UnitPrice);
