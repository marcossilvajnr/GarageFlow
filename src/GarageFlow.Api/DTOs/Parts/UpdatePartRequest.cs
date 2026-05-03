namespace GarageFlow.Api.DTOs.Parts;

public sealed record UpdatePartRequest(string Name, string UnitOfMeasure, decimal UnitPrice);
