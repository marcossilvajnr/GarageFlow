using GarageFlow.Application.Supplies.DTOs;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Supplies.Handlers;

public static class SupplyMapper
{
    public static SupplyDto ToDto(Supply supply) => new(
        supply.Id,
        supply.Name,
        supply.Code,
        supply.UnitOfMeasure,
        supply.BaseCost,
        supply.PreferredSupplierId,
        supply.IsActive,
        supply.CreatedAt,
        supply.UpdatedAt);
}
