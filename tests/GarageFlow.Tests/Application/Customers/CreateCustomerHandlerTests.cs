using FluentAssertions;
using GarageFlow.Application.Customers.Commands;
using GarageFlow.Application.Customers.Handlers;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Customers;

public sealed class CreateCustomerHandlerTests
{
    private static CreateCustomerCommand ValidCommand(string document = "529.982.247-25") => new(
        "João Silva",
        CustomerDocumentType.Cpf,
        document,
        "joao@email.com",
        "11987654321",
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsCustomerDto()
    {
        var repo = new FakeCustomerRepository();
        var handler = new CreateCustomerHandler(repo);

        var dto = await handler.HandleAsync(ValidCommand());

        dto.Should().NotBeNull();
        dto.Name.Should().Be("João Silva");
        dto.DocumentType.Should().Be(CustomerDocumentType.Cpf);
        dto.Document.Should().Be("52998224725");
        dto.IsActive.Should().BeTrue();
        dto.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithDuplicateCpf_ThrowsDuplicateDocumentException()
    {
        var repo = new FakeCustomerRepository();
        var handler = new CreateCustomerHandler(repo);

        await handler.HandleAsync(ValidCommand());
        var act = async () => await handler.HandleAsync(ValidCommand());

        await act.Should().ThrowAsync<DuplicateDocumentException>().WithMessage("CPF já cadastrado");
    }
}
