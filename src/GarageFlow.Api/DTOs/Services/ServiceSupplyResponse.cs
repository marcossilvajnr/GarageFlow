using GarageFlow.Domain.Supplies;

namespace GarageFlow.Api.DTOs.Services;

public sealed record ServiceSupplyResponse(
    Guid SupplyId,
    string SupplyName,
    decimal Quantity,
    SupplyUnit Unit);
