using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Domain.Parts;

public sealed class PartTests
{
    [Fact]
    public void Create_WithValidData_ReturnsPart()
    {
        var part = Part.Create("Filtro", "PRT-001", "SKU-001", "un", 25.00m);

        part.Code.Should().Be("PRT-001");
        part.Sku.Should().Be("SKU-001");
        part.UnitOfMeasure.Should().Be("UN");
    }

    [Fact]
    public void Create_WithInvalidUnitOfMeasure_ThrowsDomainException()
    {
        var act = () => Part.Create("Filtro", "PRT-001", "SKU-001", "INVALID", 25.00m);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidPartUnitOfMeasure);
    }

    [Fact]
    public void Create_WithInvalidSku_ThrowsDomainException()
    {
        var act = () => Part.Create("Filtro", "PRT-001", "", "UN", 25.00m);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidPartSku);
    }

    [Fact]
    public void Update_WithValidData_UpdatesUnitOfMeasure()
    {
        var part = Part.Create("Filtro", "PRT-001", "SKU-001", "UN", 25.00m);

        part.Update("Filtro Atualizado", "KG", 30m);

        part.Name.Should().Be("Filtro Atualizado");
        part.UnitOfMeasure.Should().Be("KG");
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ThrowsDomainException()
    {
        var part = Part.Create("Filtro", "PRT-001", "SKU-001", "UN", 25.00m);
        part.Deactivate();

        var act = () => part.Deactivate();
        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.PartAlreadyInactive);
    }
}
