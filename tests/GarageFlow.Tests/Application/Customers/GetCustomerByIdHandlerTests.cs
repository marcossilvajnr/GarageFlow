using FluentAssertions;
using GarageFlow.Application.Customers.Commands;
using GarageFlow.Application.Customers.Handlers;
using GarageFlow.Application.Customers.Queries;
using GarageFlow.Domain.Customers;

namespace GarageFlow.Tests.Application.Customers;

public sealed class GetCustomerByIdHandlerTests
{
    [Fact]
    public async Task Handle_ExistingCustomer_ReturnsDto()
    {
        var repo = new FakeCustomerRepository();
        var createHandler = new CreateCustomerHandler(repo);
        var dto = await createHandler.HandleAsync(new CreateCustomerCommand(
            "João Silva", CustomerDocumentType.Cpf, "529.982.247-25",
            "joao@email.com", "11987654321",
            "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100"));

        var handler = new GetCustomerByIdHandler(repo);
        var result = await handler.HandleAsync(new GetCustomerByIdQuery(dto.Id));

        result.Should().NotBeNull();
        result!.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task Handle_NonExistingCustomer_ReturnsNull()
    {
        var repo = new FakeCustomerRepository();
        var handler = new GetCustomerByIdHandler(repo);

        var result = await handler.HandleAsync(new GetCustomerByIdQuery(Guid.NewGuid()));

        result.Should().BeNull();
    }
}
