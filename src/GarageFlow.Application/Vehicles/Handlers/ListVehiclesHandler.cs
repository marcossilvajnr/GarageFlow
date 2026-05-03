using GarageFlow.Application.Vehicles.DTOs;
using GarageFlow.Application.Vehicles.Queries;
using GarageFlow.Domain.Vehicles;

namespace GarageFlow.Application.Vehicles.Handlers;

public sealed class ListVehiclesHandler(IVehicleRepository vehicleRepository)
{
    public async Task<ListVehiclesDto> HandleAsync(ListVehiclesQuery query, CancellationToken cancellationToken = default)
    {
        var (vehicles, totalCount) = await vehicleRepository.ListByCustomerIdAsync(
            query.CustomerId ?? Guid.Empty,
            query.Page,
            query.PageSize,
            cancellationToken);

        return VehicleMapper.ToListDto(vehicles, totalCount, query.Page, query.PageSize);
    }
}
