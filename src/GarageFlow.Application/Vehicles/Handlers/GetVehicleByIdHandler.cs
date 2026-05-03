using GarageFlow.Application.Vehicles.DTOs;
using GarageFlow.Application.Vehicles.Queries;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Application.Vehicles.Handlers;

public sealed class GetVehicleByIdHandler(IVehicleRepository vehicleRepository)
{
    public async Task<VehicleDto> HandleAsync(GetVehicleByIdQuery query, CancellationToken cancellationToken = default)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(query.Id, cancellationToken);
        if (vehicle == null)
            throw new EntityNotFoundException($"Veículo '{query.Id}' não encontrado");

        return VehicleMapper.ToDto(vehicle);
    }
}
