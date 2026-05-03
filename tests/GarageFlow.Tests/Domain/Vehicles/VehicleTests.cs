using FluentAssertions;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Domain.Vehicles;

public sealed class VehicleTests
{
    private static readonly Guid ValidCustomerId = Guid.NewGuid();
    private const string ValidRenavam = "11144477731";
    private const string AnotherValidRenavam = "10000000090";

    [Fact]
    public void Create_WithValidData_ReturnsActiveVehicle()
    {
        var vehicle = Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        vehicle.Id.Should().NotBeEmpty();
        vehicle.CustomerId.Should().Be(ValidCustomerId);
        vehicle.LicensePlate.Value.Should().Be("ABC1234");
        vehicle.Renavam.Value.Should().Be(ValidRenavam);
        vehicle.Make.Should().Be("Toyota");
        vehicle.Model.Should().Be("Corolla");
        vehicle.Year.Should().Be(2020);
        vehicle.Color.Should().Be("Branco");
        vehicle.IsActive.Should().BeTrue();
        vehicle.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(
            Guid.Empty,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        act.Should().Throw<DomainException>().WithMessage("Id do cliente inválido");
    }

    [Fact]
    public void Create_WithEmptyMake_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "   ",
            "Corolla",
            2020,
            "Branco");

        act.Should().Throw<DomainException>().WithMessage("Marca do veículo inválida");
    }

    [Fact]
    public void Create_WithEmptyModel_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "",
            2020,
            "Branco");

        act.Should().Throw<DomainException>().WithMessage("Modelo do veículo inválido");
    }

    [Fact]
    public void Create_WithInvalidYear_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            1800,
            "Branco");

        act.Should().Throw<DomainException>().WithMessage("Ano do veículo inválido");
    }

    [Fact]
    public void Create_WithFutureYear_ThrowsDomainException()
    {
        var futureYear = DateTime.UtcNow.Year + 5;
        var act = () => Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            futureYear,
            "Branco");

        act.Should().Throw<DomainException>().WithMessage("Ano do veículo inválido");
    }

    [Fact]
    public void Create_WithEmptyColor_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "   ");

        act.Should().Throw<DomainException>().WithMessage("Cor do veículo inválida");
    }

    [Fact]
    public void Create_WithInvalidLicensePlate_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(
            ValidCustomerId,
            "INVALID",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        act.Should().Throw<DomainException>().WithMessage("Placa inválida");
    }

    [Fact]
    public void Create_WithInvalidRenavam_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            "12345678999",
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        act.Should().Throw<DomainException>().WithMessage("RENAVAM inválido");
    }

    [Fact]
    public void Update_WithValidData_UpdatesAllowedFields()
    {
        var vehicle = Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        vehicle.Update("Honda", "Civic", 2021, "Preto");

        vehicle.Make.Should().Be("Honda");
        vehicle.Model.Should().Be("Civic");
        vehicle.Year.Should().Be(2021);
        vehicle.Color.Should().Be("Preto");
        vehicle.UpdatedAt.Should().NotBeNull();
        vehicle.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithEmptyMake_ThrowsDomainException()
    {
        var vehicle = Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        var act = () => vehicle.Update("", "Civic", 2021, "Preto");

        act.Should().Throw<DomainException>().WithMessage("Marca do veículo inválida");
    }

    [Fact]
    public void Deactivate_ActiveVehicle_SetsIsActiveFalse()
    {
        var vehicle = Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        vehicle.Deactivate();

        vehicle.IsActive.Should().BeFalse();
        vehicle.UpdatedAt.Should().NotBeNull();
        vehicle.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ThrowsDomainException()
    {
        var vehicle = Vehicle.Create(
            ValidCustomerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        vehicle.Deactivate();
        var act = () => vehicle.Deactivate();

        act.Should().Throw<DomainException>().WithMessage("Veículo já está inativo");
    }

    [Fact]
    public void Create_WithMercosulLicensePlate_ReturnsVehicle()
    {
        var vehicle = Vehicle.Create(
            ValidCustomerId,
            "ABC1D23",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        vehicle.LicensePlate.Value.Should().Be("ABC1D23");
    }
}
