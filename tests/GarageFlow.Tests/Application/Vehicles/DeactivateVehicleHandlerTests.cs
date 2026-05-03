using FluentAssertions;
using GarageFlow.Application.Vehicles.Commands;
using GarageFlow.Application.Vehicles.Handlers;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Vehicles;

public sealed class DeactivateVehicleHandlerTests
{
    private const string ValidRenavam = "11144477731";

    [Fact]
    public async Task HandleAsync_WithActiveVehicle_DeactivatesSuccessfully()
    {
        var customerId = Guid.NewGuid();
        var vehicle = Vehicle.Create(
            customerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        var vehicleRepository = new FakeVehicleRepository();
        await vehicleRepository.AddAsync(vehicle);

        var handler = new DeactivateVehicleHandler(vehicleRepository);
        var command = new DeactivateVehicleCommand(vehicle.Id);

        await handler.HandleAsync(command);

        var deactivatedVehicle = await vehicleRepository.GetByIdAsync(vehicle.Id);
        deactivatedVehicle.Should().NotBeNull();
        deactivatedVehicle!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentVehicle_ThrowsDomainException()
    {
        var vehicleId = Guid.NewGuid();
        var vehicleRepository = new FakeVehicleRepository();

        var handler = new DeactivateVehicleHandler(vehicleRepository);
        var command = new DeactivateVehicleCommand(vehicleId);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"Veículo '{vehicleId}' não encontrado");
    }
}
