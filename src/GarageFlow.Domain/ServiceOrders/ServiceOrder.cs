using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.ServiceOrders;

public sealed class ServiceOrder
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid VehicleId { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ServiceOrder() { }

    public static ServiceOrder Create(Guid customerId, Guid vehicleId)
    {
        if (customerId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderCustomerId);

        if (vehicleId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderVehicleId);

        return new ServiceOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicleId,
            Status = ServiceOrderStatus.Received,
            CreatedAt = DateTime.UtcNow
        };
    }
}
