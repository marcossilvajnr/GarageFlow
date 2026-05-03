using FluentAssertions;
using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Application.Supplies.Handlers;
using GarageFlow.Application.Supplies.Queries;

namespace GarageFlow.Tests.Application.Supplies;

public sealed class ListSuppliesHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingSupplies_ReturnsPaged()
    {
        var repo = new FakeSupplyRepository();
        var createHandler = new CreateSupplyHandler(repo);
        await createHandler.HandleAsync(new CreateSupplyCommand("Óleo Motor", "INS-001", "L", 25.00m, null));
        await createHandler.HandleAsync(new CreateSupplyCommand("Filtro de Ar", "INS-002", "UN", 15.00m, null));

        var listHandler = new ListSuppliesHandler(repo);
        var result = await listHandler.HandleAsync(new ListSuppliesQuery(1, 10));

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }
}
