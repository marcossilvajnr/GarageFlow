using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Tests.Domain.ValueObjects;

public sealed class RenavamTests
{
    [Theory]
    [InlineData("11144477731")]
    [InlineData("111.444.777-31")]
    [InlineData("111-444-777-31")]
    public void Create_WithValidRenavam_ReturnsDigitsOnly(string input)
    {
        var renavam = Renavam.Create(input);
        renavam.Value.Should().Be("11144477731");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespace_ThrowsDomainException(string? input)
    {
        var act = () => Renavam.Create(input!);
        act.Should().Throw<DomainException>().WithMessage("RENAVAM inválido");
    }

    [Theory]
    [InlineData("3509912247")]
    [InlineData("350991224760")]
    [InlineData("1234567890")]
    [InlineData("123456789")]
    public void Create_WithInvalidLength_ThrowsDomainException(string input)
    {
        var act = () => Renavam.Create(input);
        act.Should().Throw<DomainException>().WithMessage("RENAVAM inválido");
    }

    [Theory]
    [InlineData("35099122474")]
    [InlineData("35099122475")]
    [InlineData("35099122477")]
    [InlineData("11144477730")]
    public void Create_WithInvalidCheckDigit_ThrowsDomainException(string input)
    {
        var act = () => Renavam.Create(input);
        act.Should().Throw<DomainException>().WithMessage("RENAVAM inválido");
    }

    [Fact]
    public void Create_WithValidRenavam_CreatesRenavam()
    {
        var renavam = Renavam.Create("11144477731");
        renavam.Should().NotBeNull();
        renavam.Value.Should().Be("11144477731");
    }

    [Fact]
    public void Create_WithValidRenavamAndMask_NormalizesCorrectly()
    {
        var renavam = Renavam.Create("111.444.777-31");
        renavam.Value.Should().Be("11144477731");
    }

    [Fact]
    public void Create_WithValidRenavamAndHyphen_NormalizesCorrectly()
    {
        var renavam = Renavam.Create("111-444-777-31");
        renavam.Value.Should().Be("11144477731");
    }

    [Fact]
    public void Create_WithCheckDigitZero_ValidatesCorrectly()
    {
        // RENAVAM where check digit calculation results in 0
        var renavam = Renavam.Create("10000000090");
        renavam.Value.Should().Be("10000000090");
    }

    [Fact]
    public void Create_WithInvalidCheckDigitValue_ThrowsDomainException()
    {
        var act = () => Renavam.Create("11144477730");
        act.Should().Throw<DomainException>();
    }
}
