using FluentAssertions;
using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Application.Supplies.Handlers;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Supplies;

public sealed class CreateSupplyHandlerTests
{
    private static CreateSupplyCommand ValidCommand(
        string code = "INS-001",
        string name = "Óleo Motor 5W30") =>
        new(name, code, "L", 25.00m, null);

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSupplyDto()
    {
        var repo = new FakeSupplyRepository();
        var handler = new CreateSupplyHandler(repo);

        var dto = await handler.HandleAsync(ValidCommand());

        dto.Code.Should().Be("INS-001");
        dto.UnitOfMeasure.Should().Be("L");
        dto.IsActive.Should().BeTrue();
        dto.PreferredSupplierId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithPreferredSupplier_SetsSupplierId()
    {
        var repo = new FakeSupplyRepository();
        var handler = new CreateSupplyHandler(repo);
        var supplierId = Guid.NewGuid();
        var command = new CreateSupplyCommand("Óleo Motor", "INS-001", "L", 25.00m, supplierId);

        var dto = await handler.HandleAsync(command);

        dto.PreferredSupplierId.Should().Be(supplierId);
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ThrowsDuplicateSupplyDataException()
    {
        var repo = new FakeSupplyRepository();
        var handler = new CreateSupplyHandler(repo);

        await handler.HandleAsync(ValidCommand());
        var act = async () => await handler.HandleAsync(ValidCommand(code: "INS-001", name: "Outro"));

        await act.Should().ThrowAsync<DuplicateSupplyDataException>()
            .WithMessage("Código do insumo já cadastrado");
    }
}
