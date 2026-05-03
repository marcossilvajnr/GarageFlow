using FluentAssertions;
using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Application.Supplies.Handlers;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Supplies;

public sealed class UpdateSupplyHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_UpdatesSupply()
    {
        var repo = new FakeSupplyRepository();
        var createHandler = new CreateSupplyHandler(repo);
        var created = await createHandler.HandleAsync(new CreateSupplyCommand("Óleo Motor", "INS-001", "L", 25.00m, null));

        var updateHandler = new UpdateSupplyHandler(repo);
        var updated = await updateHandler.HandleAsync(new UpdateSupplyCommand(created.Id, "Óleo Motor 10W40", "KG", 30.00m, null));

        updated.Name.Should().Be("Óleo Motor 10W40");
        updated.UnitOfMeasure.Should().Be("KG");
        updated.BaseCost.Should().Be(30.00m);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSupplyRepository();
        var handler = new UpdateSupplyHandler(repo);

        var act = async () => await handler.HandleAsync(new UpdateSupplyCommand(Guid.NewGuid(), "Nome", "UN", 10.00m, null));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
