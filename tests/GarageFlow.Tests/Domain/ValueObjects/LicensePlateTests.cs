using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Tests.Domain.ValueObjects;

public sealed class LicensePlateTests
{
    [Theory]
    [InlineData("ABC1234", "ABC1234")]
    [InlineData("ABC-1234", "ABC1234")]
    [InlineData("ABC 1234", "ABC1234")]
    [InlineData("abc 1234", "ABC1234")]
    public void Create_WithValidStandardFormat_ReturnsNormalizedValue(string input, string expected)
    {
        var licensePlate = LicensePlate.Create(input);
        licensePlate.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("ABC1D23", "ABC1D23")]
    [InlineData("ABC-1D23", "ABC1D23")]
    [InlineData("ABC 1D23", "ABC1D23")]
    [InlineData("abc 1d23", "ABC1D23")]
    public void Create_WithValidMercosulFormat_ReturnsNormalizedValue(string input, string expected)
    {
        var licensePlate = LicensePlate.Create(input);
        licensePlate.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespace_ThrowsDomainException(string? input)
    {
        var act = () => LicensePlate.Create(input!);
        act.Should().Throw<DomainException>().WithMessage("Placa inválida");
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("AB12345")]
    [InlineData("ABC12345")]
    [InlineData("ABC123456")]
    [InlineData("ABCD1234")]
    public void Create_WithInvalidLength_ThrowsDomainException(string input)
    {
        var act = () => LicensePlate.Create(input);
        act.Should().Throw<DomainException>().WithMessage("Placa inválida");
    }

    [Theory]
    [InlineData("1BC1234")]
    [InlineData("A1C1234")]
    [InlineData("ABC12X4")]
    [InlineData("ABCD234")]
    public void Create_WithInvalidStandardFormat_ThrowsDomainException(string input)
    {
        var act = () => LicensePlate.Create(input);
        act.Should().Throw<DomainException>().WithMessage("Placa inválida");
    }

    [Theory]
    [InlineData("1BC1D23")]
    [InlineData("A1C1D23")]
    [InlineData("ABC11N23")]
    [InlineData("ABCL1234")]
    public void Create_WithInvalidMercosulFormat_ThrowsDomainException(string input)
    {
        var act = () => LicensePlate.Create(input);
        act.Should().Throw<DomainException>().WithMessage("Placa inválida");
    }

    [Fact]
    public void Create_WithValidStandardFormat_CreatesLicensePlate()
    {
        var licensePlate = LicensePlate.Create("ABC-1234");
        licensePlate.Should().NotBeNull();
        licensePlate.Value.Should().Be("ABC1234");
    }

    [Fact]
    public void Create_WithValidMercosulFormat_CreatesLicensePlate()
    {
        var licensePlate = LicensePlate.Create("ABC-1D23");
        licensePlate.Should().NotBeNull();
        licensePlate.Value.Should().Be("ABC1D23");
    }

    [Fact]
    public void Create_WithInvalidCharacters_ThrowsDomainException()
    {
        var act = () => LicensePlate.Create("ABC@1234");
        act.Should().Throw<DomainException>().WithMessage("Placa inválida");
    }
}
