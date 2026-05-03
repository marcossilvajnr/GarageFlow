using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Tests.Domain.ValueObjects;

public sealed class AddressTests
{
    [Fact]
    public void Create_WithValidData_ReturnsAddress()
    {
        var address = Address.Create("Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

        address.Street.Should().Be("Rua das Flores");
        address.Number.Should().Be("100");
        address.Complement.Should().BeNull();
        address.ZipCode.Should().Be("01310100");
    }

    [Fact]
    public void Create_WithZipCodeWithHyphen_NormalizesDigitsOnly()
    {
        var address = Address.Create("Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310-100");
        address.ZipCode.Should().Be("01310100");
    }

    [Fact]
    public void Create_WithLowercaseState_NormalizesToUppercase()
    {
        var address = Address.Create("Rua das Flores", "100", null, "Centro", "São Paulo", "sp", "01310100");
        address.State.Should().Be("SP");
    }

    [Fact]
    public void Create_WithInvalidState_ThrowsDomainException()
    {
        var act = () => Address.Create("Rua das Flores", "100", null, "Centro", "São Paulo", "XX", "01310100");
        act.Should().Throw<DomainException>().WithMessage("UF inválida");
    }

    [Fact]
    public void Create_WithInvalidZipCode_ThrowsDomainException()
    {
        var act = () => Address.Create("Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "0131010");
        act.Should().Throw<DomainException>().WithMessage("CEP inválido");
    }

    [Fact]
    public void Create_WithEmptyStreet_ThrowsDomainException()
    {
        var act = () => Address.Create("", "100", null, "Centro", "São Paulo", "SP", "01310100");
        act.Should().Throw<DomainException>().WithMessage("Logradouro inválido");
    }
}
