using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Domain.Services;

public sealed class ServiceCompositionPartTests
{
    private static Service CreateService() =>
        Service.Create("SRV-CPT-001", "Serviço Composição", null, 150.00m, 30);

    private static readonly Guid PartId1 = Guid.NewGuid();
    private static readonly Guid PartId2 = Guid.NewGuid();

    // ─── AddPart: success ────────────────────────────────────────────────────

    [Fact]
    public void AddPart_WithValidData_AddsToParts()
    {
        var service = CreateService();

        service.AddPart(PartId1, "Filtro de Óleo", 2);

        service.Parts.Should().HaveCount(1);
        service.Parts[0].PartId.Should().Be(PartId1);
        service.Parts[0].PartName.Should().Be("Filtro de Óleo");
        service.Parts[0].Quantity.Should().Be(2);
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddPart_MultipleDifferentParts_AddsAll()
    {
        var service = CreateService();

        service.AddPart(PartId1, "Filtro de Óleo", 1);
        service.AddPart(PartId2, "Correia Dentada", 2);

        service.Parts.Should().HaveCount(2);
    }

    // ─── AddPart: duplicate ──────────────────────────────────────────────────

    [Fact]
    public void AddPart_WithDuplicatePartId_ThrowsDuplicateServicePartException()
    {
        var service = CreateService();
        service.AddPart(PartId1, "Filtro de Óleo", 1);

        var act = () => service.AddPart(PartId1, "Filtro de Óleo", 2);

        act.Should().Throw<DuplicateServicePartException>()
            .WithMessage(DomainErrorMessages.DuplicateServicePart);
    }

    // ─── AddPart: invalid quantity ───────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void AddPart_WithInvalidQuantity_ThrowsDomainException(int quantity)
    {
        var service = CreateService();

        var act = () => service.AddPart(PartId1, "Filtro de Óleo", quantity);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServicePartQuantity);
    }

    // ─── RemovePart: success ─────────────────────────────────────────────────

    [Fact]
    public void RemovePart_WithLinkedPart_RemovesFromParts()
    {
        var service = CreateService();
        service.AddPart(PartId1, "Filtro de Óleo", 2);

        service.RemovePart(PartId1);

        service.Parts.Should().BeEmpty();
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemovePart_WithOneOfMultipleParts_RemovesOnlyTargeted()
    {
        var service = CreateService();
        service.AddPart(PartId1, "Filtro de Óleo", 1);
        service.AddPart(PartId2, "Correia Dentada", 2);

        service.RemovePart(PartId1);

        service.Parts.Should().HaveCount(1);
        service.Parts[0].PartId.Should().Be(PartId2);
    }

    // ─── RemovePart: not found ───────────────────────────────────────────────

    [Fact]
    public void RemovePart_WithUnlinkedPartId_ThrowsEntityNotFoundException()
    {
        var service = CreateService();

        var act = () => service.RemovePart(PartId1);

        act.Should().Throw<EntityNotFoundException>();
    }
}
