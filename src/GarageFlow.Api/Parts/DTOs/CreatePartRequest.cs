namespace GarageFlow.Api.Parts.DTOs;

public sealed record CreatePartRequest(
    string Name,
    string Code,
    string Sku,
    string UnitOfMeasure,
    decimal UnitPrice);
