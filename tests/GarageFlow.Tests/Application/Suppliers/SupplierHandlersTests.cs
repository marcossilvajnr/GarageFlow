using FluentAssertions;
using GarageFlow.Application.Suppliers.Commands;
using GarageFlow.Application.Suppliers.Handlers;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Suppliers;

public sealed class CreateSupplierHandlerTests
{
    private static CreateSupplierCommand ValidCommand(string cnpj = "11.222.333/0001-81") => new(
        "Fornecedor SA",
        cnpj,
        "contato@fornecedor.com",
        "11987654321",
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSupplierDto()
    {
        var repo = new FakeSupplierRepository();
        var handler = new CreateSupplierHandler(repo);

        var dto = await handler.HandleAsync(ValidCommand());

        dto.Should().NotBeNull();
        dto.Name.Should().Be("Fornecedor SA");
        dto.Cnpj.Should().Be("11222333000181");
        dto.Email.Should().Be("contato@fornecedor.com");
        dto.IsActive.Should().BeTrue();
        dto.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithDuplicateCnpj_ThrowsDuplicateSupplierDataException()
    {
        var repo = new FakeSupplierRepository();
        var handler = new CreateSupplierHandler(repo);

        await handler.HandleAsync(ValidCommand());
        var act = async () => await handler.HandleAsync(ValidCommand());

        await act.Should().ThrowAsync<DuplicateSupplierDataException>()
            .WithMessage("CNPJ já cadastrado para fornecedor");
    }
}

public sealed class GetSupplierByIdHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingId_ReturnsSupplierDto()
    {
        var command = new CreateSupplierCommand(
            "Fornecedor SA",
            "11.222.333/0001-81",
            "contato@fornecedor.com",
            "11987654321",
            "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

        var repo = new FakeSupplierRepository();
        var createHandler = new CreateSupplierHandler(repo);
        var created = await createHandler.HandleAsync(command);

        var getHandler = new GetSupplierByIdHandler(repo);
        var dto = await getHandler.HandleAsync(new GarageFlow.Application.Suppliers.Queries.GetSupplierByIdQuery(created.Id));

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(created.Id);
        dto.Name.Should().Be("Fornecedor SA");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNull()
    {
        var repo = new FakeSupplierRepository();
        var handler = new GetSupplierByIdHandler(repo);

        var dto = await handler.HandleAsync(
            new GarageFlow.Application.Suppliers.Queries.GetSupplierByIdQuery(Guid.NewGuid()));

        dto.Should().BeNull();
    }
}

public sealed class UpdateSupplierHandlerTests
{
    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSupplierRepository();
        var handler = new UpdateSupplierHandler(repo);

        var cmd = new UpdateSupplierCommand(
            Guid.NewGuid(),
            "Fornecedor Ltda",
            "novo@fornecedor.com",
            "11987654321",
            "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", "01310100");

        var act = async () => await handler.HandleAsync(cmd);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}

public sealed class DeactivateSupplierHandlerTests
{
    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakeSupplierRepository();
        var handler = new DeactivateSupplierHandler(repo);

        var act = async () => await handler.HandleAsync(
            new GarageFlow.Application.Suppliers.Commands.DeactivateSupplierCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
