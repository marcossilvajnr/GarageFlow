using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Vehicles;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class CreateServiceOrderHandler(
    IServiceOrderRepository serviceOrderRepository,
    ICustomerRepository customerRepository,
    IVehicleRepository vehicleRepository)
{
    public async Task<ServiceOrderDto> HandleAsync(CreateServiceOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.CustomerId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderCustomerId);

        if (command.VehicleId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderVehicleId);

        var customer = await customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
        if (customer is null)
            throw new EntityNotFoundException(DomainErrorMessages.CustomerNotFound(command.CustomerId));

        var vehicle = await vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle is null)
            throw new EntityNotFoundException(DomainErrorMessages.VehicleNotFound(command.VehicleId));

        if (vehicle.CustomerId != command.CustomerId)
            throw new DomainException(DomainErrorMessages.ServiceOrderVehicleCustomerMismatch);

        var serviceOrder = ServiceOrder.Create(command.CustomerId, command.VehicleId);

        await serviceOrderRepository.AddAsync(serviceOrder, cancellationToken);
        await serviceOrderRepository.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.ToDto(serviceOrder);
    }
}
