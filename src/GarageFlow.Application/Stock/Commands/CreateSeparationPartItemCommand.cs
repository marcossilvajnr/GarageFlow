namespace GarageFlow.Application.Stock.Commands;

public sealed record CreateSeparationPartItemCommand(Guid PartId, string PartName, int Quantity);
