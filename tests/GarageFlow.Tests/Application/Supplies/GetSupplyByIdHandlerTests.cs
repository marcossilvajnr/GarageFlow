using FluentAssertions;
using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Application.Supplies.Handlers;
using GarageFlow.Application.Supplies.Queries;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Supplies;

public sealed class GetSupplyByIdHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingId_ReturnsSupplyDto()
    {
        var repo = new FakeSupplyRepository();
        var createHandler = new CreateSupplyHandler(repo);
        var created = await createHandler.HandleAsync(new CreateSupplyCommand("Óleo Motor", "INS-001", "L", 25.00m, null));

        var getHandler = new GetSupplyByIdHandler(repo);
        var dto = await getHandler.HandleAsync(new GetSupplyByIdQuery(created.Id));

        dto.Should().NotBeNull();
        dto!.Code.Should().Be("INS-001");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSupplyRepository();
        var handler = new GetSupplyByIdHandler(repo);

        var act = async () => await handler.HandleAsync(new GetSupplyByIdQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
