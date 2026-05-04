using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Services.DTOs;

public sealed record ServiceSupplyItemDto(
    Guid SupplyId,
    string SupplyName,
    decimal Quantity,
    SupplyUnit Unit);
