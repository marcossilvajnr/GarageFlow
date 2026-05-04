using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Stock.Commands;

public sealed record CreateSeparationSupplyItemCommand(Guid SupplyId, string SupplyName, decimal Quantity, SupplyUnit Unit);
