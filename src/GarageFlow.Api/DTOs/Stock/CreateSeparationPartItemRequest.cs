namespace GarageFlow.Api.DTOs.Stock;

public sealed record CreateSeparationPartItemRequest(Guid PartId, string PartName, int Quantity);
