using FluentAssertions;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Tests.Domain.Employees;

public sealed class EmployeeTests
{
    private static Address ValidAddress() => Address.Create(
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

    [Fact]
    public void Create_WithValidCpf_ReturnsActiveEmployee()
    {
        var employee = Employee.Create(
            "Maria Silva",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "529.982.247-25",
            "maria@email.com",
            "11987654321",
            ValidAddress(),
            EmployeeRole.Mechanic);

        employee.Id.Should().NotBeEmpty();
        employee.Name.Should().Be("Maria Silva");
        employee.DocumentType.Should().Be(GarageFlow.Domain.Customers.CustomerDocumentType.Cpf);
        employee.Cpf.Should().NotBeNull();
        employee.Cnpj.Should().BeNull();
        employee.Role.Should().Be(EmployeeRole.Mechanic);
        employee.IsActive.Should().BeTrue();
        employee.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithValidCnpj_ReturnsActiveEmployee()
    {
        var employee = Employee.Create(
            "Carlos Pereira",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cnpj,
            "11.222.333/0001-81",
            "carlos@email.com",
            "1134567890",
            ValidAddress(),
            EmployeeRole.Administrative);

        employee.DocumentType.Should().Be(GarageFlow.Domain.Customers.CustomerDocumentType.Cnpj);
        employee.Cnpj.Should().NotBeNull();
        employee.Cpf.Should().BeNull();
        employee.Role.Should().Be(EmployeeRole.Administrative);
        employee.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsDomainException()
    {
        var act = () => Employee.Create(
            "   ",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "529.982.247-25",
            "maria@email.com",
            "11987654321",
            ValidAddress(),
            EmployeeRole.Mechanic);

        act.Should().Throw<DomainException>().WithMessage("Nome do funcionário inválido");
    }

    [Fact]
    public void Create_WithInvalidCpf_ThrowsDomainException()
    {
        var act = () => Employee.Create(
            "Maria Silva",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "111.111.111-11",
            "maria@email.com",
            "11987654321",
            ValidAddress(),
            EmployeeRole.Mechanic);

        act.Should().Throw<DomainException>().WithMessage("CPF inválido");
    }

    [Fact]
    public void Create_WithInvalidRole_ThrowsDomainException()
    {
        var act = () => Employee.Create(
            "Maria Silva",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "529.982.247-25",
            "maria@email.com",
            "11987654321",
            ValidAddress(),
            (EmployeeRole)99);

        act.Should().Throw<DomainException>().WithMessage("Cargo do funcionário inválido");
    }

    [Fact]
    public void Update_ChangesAllowedFields()
    {
        var employee = Employee.Create(
            "Maria Silva",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "529.982.247-25",
            "maria@email.com",
            "11987654321",
            ValidAddress(),
            EmployeeRole.Mechanic);

        var newAddress = Address.Create("Av. Paulista", "1000", "Apto 1", "Bela Vista", "São Paulo", "SP", "01310100");
        employee.Update("Maria Santos", "maria.santos@email.com", "11912345678", newAddress, EmployeeRole.Stockist);

        employee.Name.Should().Be("Maria Santos");
        employee.Email.Value.Should().Be("maria.santos@email.com");
        employee.Role.Should().Be(EmployeeRole.Stockist);
        employee.Cpf!.Value.Should().Be("52998224725");
        employee.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithInvalidRole_ThrowsDomainException()
    {
        var employee = Employee.Create(
            "Maria Silva",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "529.982.247-25",
            "maria@email.com",
            "11987654321",
            ValidAddress(),
            EmployeeRole.Mechanic);

        var act = () => employee.Update("Maria Santos", "maria.santos@email.com", "11912345678", ValidAddress(), (EmployeeRole)99);

        act.Should().Throw<DomainException>().WithMessage("Cargo do funcionário inválido");
    }

    [Fact]
    public void Deactivate_ActiveEmployee_SetsIsActiveFalseAndUpdatesUpdatedAt()
    {
        var employee = Employee.Create(
            "Maria Silva",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "529.982.247-25",
            "maria@email.com",
            "11987654321",
            ValidAddress(),
            EmployeeRole.Mechanic);

        employee.Deactivate();

        employee.IsActive.Should().BeFalse();
        employee.UpdatedAt.Should().NotBeNull();
        employee.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ThrowsDomainException()
    {
        var employee = Employee.Create(
            "Maria Silva",
            GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
            "529.982.247-25",
            "maria@email.com",
            "11987654321",
            ValidAddress(),
            EmployeeRole.Mechanic);

        employee.Deactivate();
        var act = () => employee.Deactivate();

        act.Should().Throw<DomainException>().WithMessage("Funcionário já está inativo");
    }

    [Fact]
    public void Create_WithInvalidDocumentType_ThrowsDomainException()
    {
        var act = () => Employee.Create(
            "Maria Silva",
            (GarageFlow.Domain.Customers.CustomerDocumentType)99,
            "529.982.247-25",
            "maria@email.com",
            "11987654321",
            ValidAddress(),
            EmployeeRole.Mechanic);

        act.Should().Throw<DomainException>().WithMessage("Tipo de documento do funcionário inválido");
    }
}