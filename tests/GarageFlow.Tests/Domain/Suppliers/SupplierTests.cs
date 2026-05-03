using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Suppliers;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Tests.Domain.Suppliers;

public sealed class SupplierTests
{
    private static Address ValidAddress() => Address.Create(
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

    [Fact]
    public void Create_WithValidCnpj_ReturnsActiveSupplier()
    {
        var supplier = Supplier.Create(
            "Fornecedor SA",
            "11.222.333/0001-81",
            "contato@fornecedor.com",
            "1134567890",
            ValidAddress());

        supplier.Id.Should().NotBeEmpty();
        supplier.Name.Should().Be("Fornecedor SA");
        supplier.Cnpj.Value.Should().Be("11222333000181");
        supplier.Email.Value.Should().Be("contato@fornecedor.com");
        supplier.PhoneNumber.Value.Should().Be("1134567890");
        supplier.IsActive.Should().BeTrue();
        supplier.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsDomainException()
    {
        var act = () => Supplier.Create(
            "   ",
            "11.222.333/0001-81",
            "contato@fornecedor.com",
            "1134567890",
            ValidAddress());

        act.Should().Throw<DomainException>().WithMessage("Nome do fornecedor inválido");
    }

    [Fact]
    public void Create_WithInvalidCnpj_ThrowsDomainException()
    {
        var act = () => Supplier.Create(
            "Fornecedor SA",
            "00.000.000/0000-00",
            "contato@fornecedor.com",
            "1134567890",
            ValidAddress());

        act.Should().Throw<DomainException>().WithMessage("CNPJ inválido");
    }

    [Fact]
    public void Create_WithInvalidEmail_ThrowsDomainException()
    {
        var act = () => Supplier.Create(
            "Fornecedor SA",
            "11.222.333/0001-81",
            "invalid-email",
            "1134567890",
            ValidAddress());

        act.Should().Throw<DomainException>().WithMessage("E-mail inválido");
    }

    [Fact]
    public void Update_ChangesAllowedFields()
    {
        var supplier = Supplier.Create(
            "Fornecedor SA",
            "11.222.333/0001-81",
            "contato@fornecedor.com",
            "1134567890",
            ValidAddress());

        var newAddress = Address.Create("Av. Paulista", "1000", "Apto 1", "Bela Vista", "São Paulo", "SP", "01310100");
        supplier.Update("Fornecedor Ltda", "novo@fornecedor.com", "11987654321", newAddress);

        supplier.Name.Should().Be("Fornecedor Ltda");
        supplier.Email.Value.Should().Be("novo@fornecedor.com");
        supplier.PhoneNumber.Value.Should().Be("11987654321");
        supplier.Cnpj.Value.Should().Be("11222333000181");
        supplier.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ActiveSupplier_SetsIsActiveFalseAndUpdatesUpdatedAt()
    {
        var supplier = Supplier.Create(
            "Fornecedor SA",
            "11.222.333/0001-81",
            "contato@fornecedor.com",
            "1134567890",
            ValidAddress());

        supplier.Deactivate();

        supplier.IsActive.Should().BeFalse();
        supplier.UpdatedAt.Should().NotBeNull();
        supplier.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ThrowsDomainException()
    {
        var supplier = Supplier.Create(
            "Fornecedor SA",
            "11.222.333/0001-81",
            "contato@fornecedor.com",
            "1134567890",
            ValidAddress());

        supplier.Deactivate();
        var act = () => supplier.Deactivate();

        act.Should().Throw<DomainException>().WithMessage("Fornecedor já está inativo");
    }
}
