using GarageFlow.Application.Vehicles.DTOs;
using GarageFlow.Domain.Vehicles;

namespace GarageFlow.Application.Vehicles.Handlers;

public static class VehicleMapper
{
    public static VehicleDto ToDto(Vehicle vehicle)
    {
        return new VehicleDto(
            vehicle.Id,
            vehicle.CustomerId,
            vehicle.LicensePlate.Value,
            vehicle.Renavam.Value,
            vehicle.Make,
            vehicle.Model,
            vehicle.Year,
            vehicle.Color,
            vehicle.IsActive,
            vehicle.CreatedAt,
            vehicle.UpdatedAt);
    }

    public static ListVehiclesDto ToListDto(
        IReadOnlyList<Vehicle> items,
        int totalCount,
        int page,
        int pageSize)
    {
        var vehicleDtos = items.Select(ToDto).ToList();
        return new ListVehiclesDto(vehicleDtos, totalCount, page, pageSize);
    }
}
