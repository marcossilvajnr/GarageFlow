namespace GarageFlow.Api.Stock.DTOs;

public sealed record CreateSeparationPartItemRequest(Guid PartId, string PartName, int Quantity);
