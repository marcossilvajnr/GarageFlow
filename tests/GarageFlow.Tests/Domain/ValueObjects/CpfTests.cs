using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Tests.Domain.ValueObjects;

public sealed class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Create_WithValidCpf_ReturnsDigitsOnly(string input)
    {
        var cpf = Cpf.Create(input);
        cpf.Value.Should().Be("52998224725");
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("00000000000")]
    [InlineData("12345678900")]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithInvalidCpf_ThrowsDomainException(string input)
    {
        var act = () => Cpf.Create(input);
        act.Should().Throw<DomainException>().WithMessage("CPF inválido");
    }

    [Fact]
    public void Create_WithNull_ThrowsDomainException()
    {
        var act = () => Cpf.Create(null!);
        act.Should().Throw<DomainException>().WithMessage("CPF inválido");
    }
}
