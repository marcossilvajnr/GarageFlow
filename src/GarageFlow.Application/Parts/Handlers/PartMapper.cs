using GarageFlow.Application.Parts.DTOs;
using GarageFlow.Domain.Parts;

namespace GarageFlow.Application.Parts.Handlers;

public static class PartMapper
{
    public static PartDto ToDto(Part part) => new(
        part.Id,
        part.Name,
        part.Code,
        part.Sku,
        part.UnitOfMeasure,
        part.UnitPrice,
        part.IsActive,
        part.CreatedAt,
        part.UpdatedAt);
}
