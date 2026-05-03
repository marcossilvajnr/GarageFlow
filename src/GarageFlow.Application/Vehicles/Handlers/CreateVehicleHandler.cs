using GarageFlow.Application.Vehicles.Commands;
using GarageFlow.Application.Vehicles.DTOs;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Vehicles.Handlers;

public sealed class CreateVehicleHandler(
    IVehicleRepository vehicleRepository,
    ICustomerRepository customerRepository)
{
    public async Task<VehicleDto> HandleAsync(CreateVehicleCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
        if (customer == null)
            throw new DomainException(DomainErrorMessages.CustomerNotFound(command.CustomerId));

        var vehicle = Vehicle.Create(
            command.CustomerId,
            command.LicensePlate,
            command.Renavam,
            command.Make,
            command.Model,
            command.Year,
            command.Color);

        await vehicleRepository.AddAsync(vehicle, cancellationToken);
        await vehicleRepository.SaveChangesAsync(cancellationToken);

        return VehicleMapper.ToDto(vehicle);
    }
}
