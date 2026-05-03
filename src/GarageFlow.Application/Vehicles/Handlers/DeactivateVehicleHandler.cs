using GarageFlow.Application.Vehicles.Commands;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Application.Vehicles.Handlers;

public sealed class DeactivateVehicleHandler(IVehicleRepository vehicleRepository)
{
    public async Task HandleAsync(DeactivateVehicleCommand command, CancellationToken cancellationToken = default)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (vehicle == null)
            throw new EntityNotFoundException($"Veículo '{command.Id}' não encontrado");

        vehicle.Deactivate();

        vehicleRepository.Update(vehicle);
        await vehicleRepository.SaveChangesAsync(cancellationToken);
    }
}
