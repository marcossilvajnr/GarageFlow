using GarageFlow.Application.Vehicles.Commands;
using GarageFlow.Application.Vehicles.DTOs;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Application.Vehicles.Handlers;

public sealed class UpdateVehicleHandler(IVehicleRepository vehicleRepository)
{
    public async Task<VehicleDto> HandleAsync(UpdateVehicleCommand command, CancellationToken cancellationToken = default)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (vehicle == null)
            throw new EntityNotFoundException($"Veículo '{command.Id}' não encontrado");

        vehicle.Update(command.Make, command.Model, command.Year, command.Color);

        vehicleRepository.Update(vehicle);
        await vehicleRepository.SaveChangesAsync(cancellationToken);

        return VehicleMapper.ToDto(vehicle);
    }
}
