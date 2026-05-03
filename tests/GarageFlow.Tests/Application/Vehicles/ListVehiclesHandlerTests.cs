using FluentAssertions;
using GarageFlow.Application.Vehicles.Handlers;
using GarageFlow.Application.Vehicles.Queries;
using GarageFlow.Domain.Vehicles;

namespace GarageFlow.Tests.Application.Vehicles;

public sealed class ListVehiclesHandlerTests
{
    private const string ValidRenavam = "11144477731";
    private const string AnotherValidRenavam = "10000000090";

    [Fact]
    public async Task HandleAsync_WithCustomerId_ReturnsPaginatedVehicles()
    {
        var customerId = Guid.NewGuid();
        var vehicles = new List<Vehicle>
        {
            Vehicle.Create(customerId, "ABC-1234", ValidRenavam, "Toyota", "Corolla", 2020, "Branco"),
            Vehicle.Create(customerId, "DEF-5678", AnotherValidRenavam, "Honda", "Civic", 2021, "Preto")
        };

        var vehicleRepository = new FakeVehicleRepository();
        foreach (var vehicle in vehicles)
            await vehicleRepository.AddAsync(vehicle);

        var handler = new ListVehiclesHandler(vehicleRepository);
        var query = new ListVehiclesQuery(customerId, 1, 10);

        var result = await handler.HandleAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task HandleAsync_WithoutCustomerId_ReturnsPaginatedVehicles()
    {
        var vehicles = new List<Vehicle>
        {
            Vehicle.Create(Guid.NewGuid(), "ABC-1234", ValidRenavam, "Toyota", "Corolla", 2020, "Branco")
        };

        var vehicleRepository = new FakeVehicleRepository();
        foreach (var vehicle in vehicles)
            await vehicleRepository.AddAsync(vehicle);

        var handler = new ListVehiclesHandler(vehicleRepository);
        var query = new ListVehiclesQuery(null, 1, 10);

        var result = await handler.HandleAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyList_ReturnsEmptyPaginatedResult()
    {
        var vehicleRepository = new FakeVehicleRepository();

        var handler = new ListVehiclesHandler(vehicleRepository);
        var query = new ListVehiclesQuery(null, 1, 10);

        var result = await handler.HandleAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
