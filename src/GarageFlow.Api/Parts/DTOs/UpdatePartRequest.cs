namespace GarageFlow.Api.Parts.DTOs;

public sealed record UpdatePartRequest(string Name, string UnitOfMeasure, decimal UnitPrice);
