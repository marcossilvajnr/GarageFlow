using FluentAssertions;
using GarageFlow.Application.Vehicles.Commands;
using GarageFlow.Application.Vehicles.Handlers;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Vehicles;

public sealed class UpdateVehicleHandlerTests
{
    private const string ValidRenavam = "11144477731";

    [Fact]
    public async Task HandleAsync_WithValidVehicleAndData_ReturnsUpdatedVehicleDto()
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

        var handler = new UpdateVehicleHandler(vehicleRepository);
        var command = new UpdateVehicleCommand(vehicle.Id, "Honda", "Civic", 2021, "Preto");

        var result = await handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.Make.Should().Be("Honda");
        result.Model.Should().Be("Civic");
        result.Year.Should().Be(2021);
        result.Color.Should().Be("Preto");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentVehicle_ThrowsDomainException()
    {
        var vehicleId = Guid.NewGuid();
        var vehicleRepository = new FakeVehicleRepository();

        var handler = new UpdateVehicleHandler(vehicleRepository);
        var command = new UpdateVehicleCommand(vehicleId, "Honda", "Civic", 2021, "Preto");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"Veículo '{vehicleId}' não encontrado");
    }
}
