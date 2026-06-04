using GarageFlow.Domain.Supplies;

namespace GarageFlow.Api.Stock.DTOs;

public sealed record CreateSeparationSupplyItemRequest(Guid SupplyId, string SupplyName, decimal Quantity, SupplyUnit Unit);
