using FluentAssertions;
using GarageFlow.Application.Vehicles.Commands;
using GarageFlow.Application.Vehicles.Handlers;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Tests.Application.Vehicles;

public sealed class CreateVehicleHandlerTests
{
    private static Address ValidAddress() => Address.Create(
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

    private const string ValidRenavam = "11144477731";
    private const string AnotherValidRenavam = "10000000090";

    [Fact]
    public async Task HandleAsync_WithValidCustomerAndVehicleData_ReturnsVehicleDto()
    {
        var customer = Customer.Create(
            "João Silva",
            CustomerDocumentType.Cpf,
            "529.982.247-25",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        var vehicleRepository = new FakeVehicleRepository();
        var customerRepository = new FakeCustomerRepository();
        await customerRepository.AddAsync(customer);

        var handler = new CreateVehicleHandler(vehicleRepository, customerRepository);
        var command = new CreateVehicleCommand(
            customer.Id,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        var result = await handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customer.Id);
        result.LicensePlate.Should().Be("ABC1234");
        result.Renavam.Should().Be(ValidRenavam);
        result.Make.Should().Be("Toyota");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentCustomer_ThrowsDomainException()
    {
        var customerId = Guid.NewGuid();
        var vehicleRepository = new FakeVehicleRepository();
        var customerRepository = new FakeCustomerRepository();

        var handler = new CreateVehicleHandler(vehicleRepository, customerRepository);
        var command = new CreateVehicleCommand(
            customerId,
            "ABC-1234",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"Cliente '{customerId}' não encontrado");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidLicensePlate_ThrowsDomainException()
    {
        var customer = Customer.Create(
            "João Silva",
            CustomerDocumentType.Cpf,
            "529.982.247-25",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        var vehicleRepository = new FakeVehicleRepository();
        var customerRepository = new FakeCustomerRepository();
        await customerRepository.AddAsync(customer);

        var handler = new CreateVehicleHandler(vehicleRepository, customerRepository);
        var command = new CreateVehicleCommand(
            customer.Id,
            "INVALID",
            ValidRenavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Placa inválida");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateLicensePlate_ThrowsDomainException()
    {
        var customer = Customer.Create(
            "João Silva",
            CustomerDocumentType.Cpf,
            "529.982.247-25",
            "joao@email.com",
            "11987654321",
            ValidAddress());

        var vehicleRepository = new FakeVehicleRepository();
        var customerRepository = new FakeCustomerRepository();
        await customerRepository.AddAsync(customer);

        var vehicle = Vehicle.Create(customer.Id, "ABC-1234", ValidRenavam, "Toyota", "Corolla", 2020, "Branco");
        await vehicleRepository.AddAsync(vehicle);

        var handler = new CreateVehicleHandler(vehicleRepository, customerRepository);
        var command = new CreateVehicleCommand(
            customer.Id,
            "ABC-1234",
            AnotherValidRenavam,
            "Honda",
            "Civic",
            2021,
            "Preto");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Placa já cadastrada");
    }
}
