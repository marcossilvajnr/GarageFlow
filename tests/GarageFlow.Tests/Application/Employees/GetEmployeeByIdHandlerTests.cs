using FluentAssertions;
using GarageFlow.Application.Employees.Enums;
using GarageFlow.Application.Employees.Handlers;
using GarageFlow.Application.Employees.Queries;
using GarageFlow.Domain.ValueObjects;

using DomainEmployee = GarageFlow.Domain.Employees.Employee;
using DomainEmployeeRole = GarageFlow.Domain.Employees.EmployeeRole;

namespace GarageFlow.Tests.Application.Employees;

public sealed class GetEmployeeByIdHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingEmployee_ReturnsEmployeeDto()
    {
        var repo = new FakeEmployeeRepository();
        var address = Address.Create("Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");
        var employee = DomainEmployee.Create(
            "Maria Silva",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "529.982.247-25",
            "maria@email.com",
            "11987654321",
            address,
            DomainEmployeeRole.Mechanic);

        await repo.AddAsync(employee);
        var handler = new GetEmployeeByIdHandler(repo);

        var dto = await handler.HandleAsync(new GetEmployeeByIdQuery(employee.Id));

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(employee.Id);
        dto.Name.Should().Be("Maria Silva");
        dto.Role.Should().Be(EmployeeRole.Mechanic);
    }

    [Fact]
    public async Task Handle_WithNonExistingEmployee_ReturnsNull()
    {
        var repo = new FakeEmployeeRepository();
        var handler = new GetEmployeeByIdHandler(repo);

        var dto = await handler.HandleAsync(new GetEmployeeByIdQuery(Guid.NewGuid()));

        dto.Should().BeNull();
    }
}
