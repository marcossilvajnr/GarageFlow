namespace GarageFlow.Application.Stock.DTOs;

public sealed record SeparationPartItemDto(
    Guid PartId,
    string PartName,
    int Quantity,
    bool IsReserved);
