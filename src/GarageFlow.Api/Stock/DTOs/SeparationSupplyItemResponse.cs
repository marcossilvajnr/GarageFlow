using GarageFlow.Domain.Supplies;

namespace GarageFlow.Api.Stock.DTOs;

public sealed record SeparationSupplyItemResponse(
    Guid SupplyId,
    string SupplyName,
    decimal Quantity,
    SupplyUnit Unit,
    bool IsReserved);
