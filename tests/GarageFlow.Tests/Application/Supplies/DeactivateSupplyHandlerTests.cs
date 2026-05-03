using FluentAssertions;
using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Application.Supplies.Handlers;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Supplies;

public sealed class DeactivateSupplyHandlerTests
{
    [Fact]
    public async Task Handle_WithActiveSupply_DeactivatesIt()
    {
        var repo = new FakeSupplyRepository();
        var createHandler = new CreateSupplyHandler(repo);
        var created = await createHandler.HandleAsync(new CreateSupplyCommand("Óleo Motor", "INS-001", "L", 25.00m, null));

        var deactivateHandler = new DeactivateSupplyHandler(repo);
        await deactivateHandler.HandleAsync(new DeactivateSupplyCommand(created.Id));

        var supply = repo.All.First(s => s.Id == created.Id);
        supply.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSupplyRepository();
        var handler = new DeactivateSupplyHandler(repo);

        var act = async () => await handler.HandleAsync(new DeactivateSupplyCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
