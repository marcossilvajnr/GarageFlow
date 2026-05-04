namespace GarageFlow.Api.DTOs.Stock;

public sealed record SeparationPartItemResponse(
    Guid PartId,
    string PartName,
    int Quantity,
    bool IsReserved);
