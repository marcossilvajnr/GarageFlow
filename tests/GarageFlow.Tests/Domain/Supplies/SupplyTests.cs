using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Tests.Domain.Supplies;

public sealed class SupplyTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSupply()
    {
        var supply = Supply.Create("Óleo Motor 5W30", "INS-001", "L", 25.00m);

        supply.Code.Should().Be("INS-001");
        supply.UnitOfMeasure.Should().Be("L");
        supply.IsActive.Should().BeTrue();
        supply.PreferredSupplierId.Should().BeNull();
    }

    [Fact]
    public void Create_WithPreferredSupplier_SetsPreferredSupplierId()
    {
        var supplierId = Guid.NewGuid();
        var supply = Supply.Create("Óleo Motor 5W30", "INS-001", "L", 25.00m, supplierId);

        supply.PreferredSupplierId.Should().Be(supplierId);
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsDomainException()
    {
        var act = () => Supply.Create("", "INS-001", "L", 25.00m);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidSupplyName);
    }

    [Fact]
    public void Create_WithEmptyCode_ThrowsDomainException()
    {
        var act = () => Supply.Create("Óleo Motor", "", "L", 25.00m);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidSupplyCode);
    }

    [Fact]
    public void Create_WithInvalidUnitOfMeasure_ThrowsDomainException()
    {
        var act = () => Supply.Create("Óleo Motor", "INS-001", "INVALID", 25.00m);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidSupplyUnitOfMeasure);
    }

    [Fact]
    public void Create_WithNegativeBaseCost_ThrowsDomainException()
    {
        var act = () => Supply.Create("Óleo Motor", "INS-001", "L", -1.00m);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidSupplyBaseCost);
    }

    [Fact]
    public void Create_WithZeroBaseCost_Succeeds()
    {
        var supply = Supply.Create("Óleo Motor", "INS-001", "L", 0m);

        supply.BaseCost.Should().Be(0m);
    }

    [Fact]
    public void Create_TrimsNameAndCode()
    {
        var supply = Supply.Create("  Óleo Motor  ", "  INS-001  ", "L", 25.00m);

        supply.Name.Should().Be("Óleo Motor");
        supply.Code.Should().Be("INS-001");
    }

    [Fact]
    public void Create_NormalizesUnitOfMeasureToUpperCase()
    {
        var supply = Supply.Create("Óleo Motor", "INS-001", "l", 25.00m);

        supply.UnitOfMeasure.Should().Be("L");
    }

    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        var supply = Supply.Create("Óleo Motor 5W30", "INS-001", "L", 25.00m);
        var newSupplierId = Guid.NewGuid();

        supply.Update("Óleo Motor 10W40", "KG", 30.00m, newSupplierId);

        supply.Name.Should().Be("Óleo Motor 10W40");
        supply.UnitOfMeasure.Should().Be("KG");
        supply.BaseCost.Should().Be(30.00m);
        supply.PreferredSupplierId.Should().Be(newSupplierId);
        supply.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithInvalidUnitOfMeasure_ThrowsDomainException()
    {
        var supply = Supply.Create("Óleo Motor", "INS-001", "L", 25.00m);

        var act = () => supply.Update("Óleo Motor", "INVALID", 25.00m, null);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidSupplyUnitOfMeasure);
    }

    [Fact]
    public void Deactivate_ActiveSupply_SetsIsActiveFalse()
    {
        var supply = Supply.Create("Óleo Motor", "INS-001", "L", 25.00m);

        supply.Deactivate();

        supply.IsActive.Should().BeFalse();
        supply.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ThrowsDomainException()
    {
        var supply = Supply.Create("Óleo Motor", "INS-001", "L", 25.00m);
        supply.Deactivate();

        var act = () => supply.Deactivate();

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.SupplyAlreadyInactive);
    }
}
