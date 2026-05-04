using GarageFlow.Application.Services.DTOs;
using GarageFlow.Domain.Services;

namespace GarageFlow.Application.Services.Handlers;

public static class ServiceMapper
{
    public static ServiceDto ToDto(Service service) => new(
        service.Id,
        service.Code,
        service.Name,
        service.Description,
        service.BasePrice,
        service.EstimatedDurationMinutes,
        service.IsActive,
        service.CreatedAt,
        service.UpdatedAt,
        service.Parts.Select(p => new ServicePartItemDto(p.PartId, p.PartName, p.Quantity)).ToList());
}
