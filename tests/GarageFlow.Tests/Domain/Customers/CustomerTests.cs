using FluentAssertions;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Tests.Domain.Customers;

public sealed class CustomerTests
{
    private static Address ValidAddress() => Address.Create(
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

    [Fact]
    public void Create_WithValidCpf_ReturnsActiveCustomer()
    {
        var customer = Customer.Create(
            "João Silva",
            CustomerDocumentType.Cpf,
            "529.982.247-25",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        customer.Id.Should().NotBeEmpty();
        customer.Name.Should().Be("João Silva");
        customer.DocumentType.Should().Be(CustomerDocumentType.Cpf);
        customer.Cpf.Should().NotBeNull();
        customer.Cnpj.Should().BeNull();
        customer.IsActive.Should().BeTrue();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithValidCnpj_ReturnsActiveCustomer()
    {
        var customer = Customer.Create(
            "Empresa SA",
            CustomerDocumentType.Cnpj,
            "11.222.333/0001-81",
            "contato@empresa.com",
            "1134567890",
            ValidAddress());

        customer.DocumentType.Should().Be(CustomerDocumentType.Cnpj);
        customer.Cnpj.Should().NotBeNull();
        customer.Cpf.Should().BeNull();
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsDomainException()
    {
        var act = () => Customer.Create(
            "   ",
            CustomerDocumentType.Cpf,
            "529.982.247-25",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        act.Should().Throw<DomainException>().WithMessage("Nome do cliente inválido");
    }

    [Fact]
    public void Create_WithInvalidCpf_ThrowsDomainException()
    {
        var act = () => Customer.Create(
            "João Silva",
            CustomerDocumentType.Cpf,
            "111.111.111-11",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        act.Should().Throw<DomainException>().WithMessage("CPF inválido");
    }

    [Fact]
    public void Update_ChangesAllowedFields()
    {
        var customer = Customer.Create(
            "João Silva",
            CustomerDocumentType.Cpf,
            "529.982.247-25",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        var newAddress = Address.Create("Av. Paulista", "1000", "Apto 1", "Bela Vista", "São Paulo", "SP", "01310100");
        customer.Update("João Santos", "joao.santos@email.com", "11912345678", newAddress);

        customer.Name.Should().Be("João Santos");
        customer.Email.Value.Should().Be("joao.santos@email.com");
        customer.Cpf!.Value.Should().Be("52998224725");
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ActiveCustomer_SetsIsActiveFalseAndUpdatesUpdatedAt()
    {
        var customer = Customer.Create(
            "João Silva",
            CustomerDocumentType.Cpf,
            "529.982.247-25",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        customer.Deactivate();

        customer.IsActive.Should().BeFalse();
        customer.UpdatedAt.Should().NotBeNull();
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ThrowsDomainException()
    {
        var customer = Customer.Create(
            "João Silva",
            CustomerDocumentType.Cpf,
            "529.982.247-25",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        customer.Deactivate();
        var act = () => customer.Deactivate();

        act.Should().Throw<DomainException>().WithMessage("Cliente já está inativo");
    }

    [Fact]
    public void Create_WithInvalidDocumentType_ThrowsDomainException()
    {
        var act = () => Customer.Create(
            "João Silva",
            (CustomerDocumentType)99,
            "529.982.247-25",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        act.Should().Throw<DomainException>().WithMessage("Tipo de documento do cliente inválido");
    }
}
