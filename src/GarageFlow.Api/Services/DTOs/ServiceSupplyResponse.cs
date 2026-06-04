using GarageFlow.Domain.Supplies;

namespace GarageFlow.Api.Services.DTOs;

public sealed record ServiceSupplyResponse(
    Guid SupplyId,
    string SupplyName,
    decimal Quantity,
    SupplyUnit Unit);
