using FluentAssertions;
using GarageFlow.Application.Vehicles.Handlers;
using GarageFlow.Application.Vehicles.Queries;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Vehicles;

public sealed class GetVehicleByIdHandlerTests
{
    private const string ValidRenavam = "11144477731";

    [Fact]
    public async Task HandleAsync_WithExistentVehicle_ReturnsVehicleDto()
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

        var handler = new GetVehicleByIdHandler(vehicleRepository);
        var query = new GetVehicleByIdQuery(vehicle.Id);

        var result = await handler.HandleAsync(query);

        result.Should().NotBeNull();
        result.Id.Should().Be(vehicle.Id);
        result.CustomerId.Should().Be(customerId);
        result.LicensePlate.Should().Be("ABC1234");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentVehicle_ThrowsDomainException()
    {
        var vehicleId = Guid.NewGuid();
        var vehicleRepository = new FakeVehicleRepository();

        var handler = new GetVehicleByIdHandler(vehicleRepository);
        var query = new GetVehicleByIdQuery(vehicleId);

        var act = async () => await handler.HandleAsync(query);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"Veículo '{vehicleId}' não encontrado");
    }
}
