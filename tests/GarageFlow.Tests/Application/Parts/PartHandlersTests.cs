using FluentAssertions;
using GarageFlow.Application.Parts.Commands;
using GarageFlow.Application.Parts.Handlers;
using GarageFlow.Application.Parts.Queries;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Parts;

public sealed class CreatePartHandlerTests
{
    private static CreatePartCommand ValidCommand(
        string code = "PRT-001",
        string sku = "SKU-001",
        string name = "Filtro de Óleo") =>
        new(name, code, sku, "UN", 25.00m);

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsPartDto()
    {
        var repo = new FakePartRepository();
        var handler = new CreatePartHandler(repo);

        var dto = await handler.HandleAsync(ValidCommand());

        dto.Code.Should().Be("PRT-001");
        dto.Sku.Should().Be("SKU-001");
        dto.UnitOfMeasure.Should().Be("UN");
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ThrowsDuplicatePartDataException()
    {
        var repo = new FakePartRepository();
        var handler = new CreatePartHandler(repo);

        await handler.HandleAsync(ValidCommand());
        var act = async () => await handler.HandleAsync(ValidCommand(code: "PRT-001", sku: "SKU-002", name: "Outro"));

        await act.Should().ThrowAsync<DuplicatePartDataException>()
            .WithMessage("Código da peça já cadastrado");
    }

    [Fact]
    public async Task Handle_WithDuplicateSku_ThrowsDuplicatePartDataException()
    {
        var repo = new FakePartRepository();
        var handler = new CreatePartHandler(repo);

        await handler.HandleAsync(ValidCommand());
        var act = async () => await handler.HandleAsync(ValidCommand(code: "PRT-002", sku: "SKU-001", name: "Outro"));

        await act.Should().ThrowAsync<DuplicatePartDataException>()
            .WithMessage("SKU da peça já cadastrado");
    }
}

public sealed class GetPartByIdHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingId_ReturnsPartDto()
    {
        var repo = new FakePartRepository();
        var createHandler = new CreatePartHandler(repo);
        var created = await createHandler.HandleAsync(new CreatePartCommand("Filtro", "PRT-001", "SKU-001", "UN", 25.00m));

        var getHandler = new GetPartByIdHandler(repo);
        var dto = await getHandler.HandleAsync(new GetPartByIdQuery(created.Id));

        dto.Should().NotBeNull();
        dto!.Sku.Should().Be("SKU-001");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakePartRepository();
        var handler = new GetPartByIdHandler(repo);

        var act = async () => await handler.HandleAsync(new GetPartByIdQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}

public sealed class ListPartsHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingParts_ReturnsPaged()
    {
        var repo = new FakePartRepository();
        var createHandler = new CreatePartHandler(repo);
        await createHandler.HandleAsync(new CreatePartCommand("Filtro", "PRT-001", "SKU-001", "UN", 25.00m));
        await createHandler.HandleAsync(new CreatePartCommand("Óleo", "PRT-002", "SKU-002", "L", 15.00m));

        var listHandler = new ListPartsHandler(repo);
        var result = await listHandler.HandleAsync(new ListPartsQuery(1, 10));

        result.Items.Should().HaveCount(2);
    }
}

public sealed class UpdatePartHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_UpdatesPart()
    {
        var repo = new FakePartRepository();
        var createHandler = new CreatePartHandler(repo);
        var created = await createHandler.HandleAsync(new CreatePartCommand("Filtro", "PRT-001", "SKU-001", "UN", 25.00m));

        var updateHandler = new UpdatePartHandler(repo);
        var updated = await updateHandler.HandleAsync(new UpdatePartCommand(created.Id, "Filtro Atualizado", "KG", 30.00m));

        updated.Name.Should().Be("Filtro Atualizado");
        updated.UnitOfMeasure.Should().Be("KG");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakePartRepository();
        var handler = new UpdatePartHandler(repo);

        var act = async () => await handler.HandleAsync(new UpdatePartCommand(Guid.NewGuid(), "Nome", "UN", 10.00m));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}

public sealed class DeactivatePartHandlerTests
{
    [Fact]
    public async Task Handle_WithAlreadyInactivePart_ThrowsDomainException()
    {
        var repo = new FakePartRepository();
        var createHandler = new CreatePartHandler(repo);
        var created = await createHandler.HandleAsync(new CreatePartCommand("Filtro", "PRT-001", "SKU-001", "UN", 25.00m));

        var deactivateHandler = new DeactivatePartHandler(repo);
        await deactivateHandler.HandleAsync(new DeactivatePartCommand(created.Id));

        var act = async () => await deactivateHandler.HandleAsync(new DeactivatePartCommand(created.Id));
        await act.Should().ThrowAsync<DomainException>();
    }
}
