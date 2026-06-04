using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Application.Stock.DTOs;

public sealed record SeparationSupplyItemDto(
    Guid SupplyId,
    string SupplyName,
    decimal Quantity,
    SupplyUnit Unit,
    bool IsReserved);
