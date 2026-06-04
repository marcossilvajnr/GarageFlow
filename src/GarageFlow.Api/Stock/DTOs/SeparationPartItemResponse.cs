namespace GarageFlow.Api.Stock.DTOs;

public sealed record SeparationPartItemResponse(
    Guid PartId,
    string PartName,
    int Quantity,
    bool IsReserved);
