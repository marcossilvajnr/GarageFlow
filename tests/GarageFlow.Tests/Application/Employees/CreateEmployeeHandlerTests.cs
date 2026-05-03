using FluentAssertions;
using GarageFlow.Application.Employees.Commands;
using GarageFlow.Application.Employees.Handlers;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Employees;

public sealed class CreateEmployeeHandlerTests
{
    private static CreateEmployeeCommand ValidCommand(string document = "529.982.247-25") => new(
        "Maria Silva",
        GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
        document,
        "maria@email.com",
        "11987654321",
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100",
        EmployeeRole.Mechanic);

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsEmployeeDto()
    {
        var repo = new FakeEmployeeRepository();
        var handler = new CreateEmployeeHandler(repo);

        var dto = await handler.HandleAsync(ValidCommand());

        dto.Should().NotBeNull();
        dto.Name.Should().Be("Maria Silva");
        dto.DocumentType.Should().Be(GarageFlow.Domain.Customers.CustomerDocumentType.Cpf);
        dto.Document.Should().Be("52998224725");
        dto.Role.Should().Be(EmployeeRole.Mechanic);
        dto.IsActive.Should().BeTrue();
        dto.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithDuplicateCpf_ThrowsDuplicateDocumentException()
    {
        var repo = new FakeEmployeeRepository();
        var handler = new CreateEmployeeHandler(repo);

        await handler.HandleAsync(ValidCommand());
        var act = async () => await handler.HandleAsync(ValidCommand());

        await act.Should().ThrowAsync<DuplicateDocumentException>().WithMessage("CPF já cadastrado para funcionário");
    }
}