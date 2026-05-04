using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Tests.Domain.Services;

public sealed class ServiceCompositionSupplyTests
{
    private static Service CreateService() =>
        Service.Create("SRV-CSP-001", "Serviço Composição Insumo", null, 200.00m, 60);

    private static readonly Guid SupplyId1 = Guid.NewGuid();
    private static readonly Guid SupplyId2 = Guid.NewGuid();

    // ─── AddSupply: success ──────────────────────────────────────────────────

    [Fact]
    public void AddSupply_WithValidData_AddsToSupplies()
    {
        var service = CreateService();

        service.AddSupply(SupplyId1, "Óleo Motor", 1.5m, SupplyUnit.Liter);

        service.Supplies.Should().HaveCount(1);
        service.Supplies[0].SupplyId.Should().Be(SupplyId1);
        service.Supplies[0].SupplyName.Should().Be("Óleo Motor");
        service.Supplies[0].Quantity.Should().Be(1.5m);
        service.Supplies[0].Unit.Should().Be(SupplyUnit.Liter);
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddSupply_MultipleDifferentSupplies_AddsAll()
    {
        var service = CreateService();

        service.AddSupply(SupplyId1, "Óleo Motor", 1.5m, SupplyUnit.Liter);
        service.AddSupply(SupplyId2, "Fluido de Freio", 500m, SupplyUnit.Milliliter);

        service.Supplies.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(SupplyUnit.Liter)]
    [InlineData(SupplyUnit.Milliliter)]
    [InlineData(SupplyUnit.Gram)]
    [InlineData(SupplyUnit.Kilogram)]
    [InlineData(SupplyUnit.Unit)]
    public void AddSupply_WithEachCanonicalUnit_Succeeds(SupplyUnit unit)
    {
        var service = CreateService();

        service.AddSupply(SupplyId1, "Insumo Teste", 1m, unit);

        service.Supplies[0].Unit.Should().Be(unit);
    }

    // ─── AddSupply: duplicate ────────────────────────────────────────────────

    [Fact]
    public void AddSupply_WithDuplicateSupplyId_ThrowsDuplicateServiceSupplyException()
    {
        var service = CreateService();
        service.AddSupply(SupplyId1, "Óleo Motor", 1.5m, SupplyUnit.Liter);

        var act = () => service.AddSupply(SupplyId1, "Óleo Motor", 2m, SupplyUnit.Liter);

        act.Should().Throw<DuplicateServiceSupplyException>()
            .WithMessage(DomainErrorMessages.DuplicateServiceSupply);
    }

    // ─── AddSupply: invalid quantity ─────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(-10)]
    public void AddSupply_WithInvalidQuantity_ThrowsDomainException(decimal quantity)
    {
        var service = CreateService();

        var act = () => service.AddSupply(SupplyId1, "Óleo Motor", quantity, SupplyUnit.Liter);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceSupplyQuantity);
    }

    // ─── RemoveSupply: success ───────────────────────────────────────────────

    [Fact]
    public void RemoveSupply_WithLinkedSupply_RemovesFromSupplies()
    {
        var service = CreateService();
        service.AddSupply(SupplyId1, "Óleo Motor", 1.5m, SupplyUnit.Liter);

        service.RemoveSupply(SupplyId1);

        service.Supplies.Should().BeEmpty();
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemoveSupply_WithOneOfMultipleSupplies_RemovesOnlyTargeted()
    {
        var service = CreateService();
        service.AddSupply(SupplyId1, "Óleo Motor", 1.5m, SupplyUnit.Liter);
        service.AddSupply(SupplyId2, "Fluido de Freio", 500m, SupplyUnit.Milliliter);

        service.RemoveSupply(SupplyId1);

        service.Supplies.Should().HaveCount(1);
        service.Supplies[0].SupplyId.Should().Be(SupplyId2);
    }

    // ─── RemoveSupply: not found ─────────────────────────────────────────────

    [Fact]
    public void RemoveSupply_WithUnlinkedSupplyId_ThrowsEntityNotFoundException()
    {
        var service = CreateService();

        var act = () => service.RemoveSupply(SupplyId1);

        act.Should().Throw<EntityNotFoundException>();
    }
}
