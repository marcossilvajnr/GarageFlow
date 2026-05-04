using GarageFlow.Domain.Supplies;

namespace GarageFlow.Api.DTOs.Stock;

public sealed record CreateSeparationSupplyItemRequest(Guid SupplyId, string SupplyName, decimal Quantity, SupplyUnit Unit);
