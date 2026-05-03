using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Domain.Services;

public sealed class ServiceTests
{
    // ─── Create: success ────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsService()
    {
        var service = Service.Create("SRV-001", "Troca de Óleo", "Serviço completo", 150.00m, 30);

        service.Id.Should().NotBeEmpty();
        service.Code.Should().Be("SRV-001");
        service.Name.Should().Be("Troca de Óleo");
        service.Description.Should().Be("Serviço completo");
        service.BasePrice.Should().Be(150.00m);
        service.EstimatedDurationMinutes.Should().Be(30);
        service.IsActive.Should().BeTrue();
        service.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        service.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithoutDescription_ReturnsServiceWithNullDescription()
    {
        var service = Service.Create("SRV-001", "Troca de Óleo", null, 150.00m, null);

        service.Description.Should().BeNull();
        service.EstimatedDurationMinutes.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsWhitespace_OnCodeAndName()
    {
        var service = Service.Create("  SRV-001  ", "  Troca de Óleo  ", null, 150.00m, null);

        service.Code.Should().Be("SRV-001");
        service.Name.Should().Be("Troca de Óleo");
    }

    // ─── Create: invalid code ────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidCode_ThrowsDomainException(string? code)
    {
        var act = () => Service.Create(code!, "Troca de Óleo", null, 150.00m, null);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceCode);
    }

    [Fact]
    public void Create_WithCodeExceedingMaxLength_ThrowsDomainException()
    {
        var longCode = new string('X', 51);
        var act = () => Service.Create(longCode, "Troca de Óleo", null, 150.00m, null);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceCode);
    }

    // ─── Create: invalid name ────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ThrowsDomainException(string? name)
    {
        var act = () => Service.Create("SRV-001", name!, null, 150.00m, null);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceName);
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ThrowsDomainException()
    {
        var longName = new string('N', 201);
        var act = () => Service.Create("SRV-001", longName, null, 150.00m, null);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceName);
    }

    // ─── Create: invalid base price ─────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_WithBasePriceNotGreaterThanZero_ThrowsDomainException(decimal price)
    {
        var act = () => Service.Create("SRV-001", "Troca de Óleo", null, price, null);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceBasePrice);
    }

    // ─── Create: invalid estimated duration ─────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithInvalidEstimatedDuration_ThrowsDomainException(int duration)
    {
        var act = () => Service.Create("SRV-001", "Troca de Óleo", null, 150.00m, duration);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceEstimatedDuration);
    }

    // ─── Update: success ────────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        var service = Service.Create("SRV-001", "Troca de Óleo", null, 150.00m, 30);

        service.Update("Troca de Óleo Premium", "Descrição atualizada", 200.00m, 45);

        service.Name.Should().Be("Troca de Óleo Premium");
        service.Description.Should().Be("Descrição atualizada");
        service.BasePrice.Should().Be(200.00m);
        service.EstimatedDurationMinutes.Should().Be(45);
        service.Code.Should().Be("SRV-001"); // imutável
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithInvalidBasePrice_ThrowsDomainException()
    {
        var service = Service.Create("SRV-001", "Troca de Óleo", null, 150.00m, null);

        var act = () => service.Update("Troca de Óleo", null, 0, null);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceBasePrice);
    }

    [Fact]
    public void Update_WithEmptyName_ThrowsDomainException()
    {
        var service = Service.Create("SRV-001", "Troca de Óleo", null, 150.00m, null);

        var act = () => service.Update("", null, 150.00m, null);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceName);
    }

    // ─── Deactivate ──────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_ActiveService_SetsIsActiveFalse()
    {
        var service = Service.Create("SRV-001", "Troca de Óleo", null, 150.00m, null);

        service.Deactivate();

        service.IsActive.Should().BeFalse();
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_AlreadyInactiveService_ThrowsDomainException()
    {
        var service = Service.Create("SRV-001", "Troca de Óleo", null, 150.00m, null);
        service.Deactivate();

        var act = () => service.Deactivate();

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.ServiceAlreadyInactive);
    }
}
