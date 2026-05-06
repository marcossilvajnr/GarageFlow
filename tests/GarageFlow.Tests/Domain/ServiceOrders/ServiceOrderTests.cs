using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Tests.Domain.ServiceOrders;

public sealed class ServiceOrderTests
{
    [Fact]
    public void Create_WithValidIds_ReturnsServiceOrderWithStatusReceived()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var serviceOrder = ServiceOrder.Create(customerId, vehicleId);

        serviceOrder.Id.Should().NotBeEmpty();
        serviceOrder.CustomerId.Should().Be(customerId);
        serviceOrder.VehicleId.Should().Be(vehicleId);
        serviceOrder.Status.Should().Be(ServiceOrderStatus.Received);
        serviceOrder.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        serviceOrder.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_StatusIsAlwaysReceived()
    {
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        serviceOrder.Status.Should().Be(ServiceOrderStatus.Received);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ThrowsDomainException()
    {
        var act = () => ServiceOrder.Create(Guid.Empty, Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("Id do cliente da OS inválido");
    }

    [Fact]
    public void Create_WithEmptyVehicleId_ThrowsDomainException()
    {
        var act = () => ServiceOrder.Create(Guid.NewGuid(), Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("Id do veículo da OS inválido");
    }

    [Fact]
    public void Create_CustomerIdIsImmutableAfterCreation()
    {
        var customerId = Guid.NewGuid();
        var serviceOrder = ServiceOrder.Create(customerId, Guid.NewGuid());

        serviceOrder.CustomerId.Should().Be(customerId);

        var setter = typeof(ServiceOrder).GetProperty(nameof(ServiceOrder.CustomerId))!.SetMethod;
        (setter is null || !setter.IsPublic).Should().BeTrue("CustomerId deve ter setter privado ou nulo");
    }

    [Fact]
    public void Create_VehicleIdIsImmutableAfterCreation()
    {
        var vehicleId = Guid.NewGuid();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), vehicleId);

        serviceOrder.VehicleId.Should().Be(vehicleId);

        var setter = typeof(ServiceOrder).GetProperty(nameof(ServiceOrder.VehicleId))!.SetMethod;
        (setter is null || !setter.IsPublic).Should().BeTrue("VehicleId deve ter setter privado ou nulo");
    }

    [Fact]
    public void Deliver_WhenStatusIsNotFinished_ThrowsInvalidServiceOrderStatusTransitionException()
    {
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => serviceOrder.Deliver();

        act.Should().Throw<InvalidServiceOrderStatusTransitionException>()
            .WithMessage("A Ordem de Serviço não está Finalizada e não pode ser entregue");
    }

    [Fact]
    public void Deliver_WhenStatusIsFinished_ChangesStatusToDelivered()
    {
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        typeof(ServiceOrder).GetProperty(nameof(ServiceOrder.Status))!
            .SetValue(serviceOrder, ServiceOrderStatus.Finished);

        serviceOrder.Deliver();

        serviceOrder.Status.Should().Be(ServiceOrderStatus.Delivered);
    }
}
