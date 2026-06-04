using FluentAssertions;
using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.Handlers;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.ValueObjects;
using GarageFlow.Domain.Vehicles;
using GarageFlow.Tests.Application.Employees;
using AppServiceOrderStatus = GarageFlow.Application.ServiceOrders.Enums.ServiceOrderStatus;

namespace GarageFlow.Tests.Application.ServiceOrders;

public sealed class ServiceOrderHandlersTests
{
    private static Address ValidAddress() => Address.Create(
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

    private static Customer ValidCustomer() => Customer.Create(
        "João Silva", CustomerDocumentType.Cpf, "529.982.247-25",
        "joao@email.com", "11987654321", ValidAddress());

    private static Vehicle ValidVehicle(Guid customerId) => Vehicle.Create(
        customerId, "ABC-1234", "11144477731", "Toyota", "Corolla", 2020, "Branco");

    private static async Task<Employee> SeedFrontDeskEmployeeAsync(FakeEmployeeRepository employeeRepo)
    {
        var employee = Employee.Create(
            "Atendente Teste",
            CustomerDocumentType.Cpf,
            "529.982.247-25",
            "atendente@garageflow.com",
            "11987654321",
            ValidAddress(),
            EmployeeRole.Attendant);

        await employeeRepo.AddAsync(employee);
        return employee;
    }

    // Create handler tests

    [Fact]
    public async Task CreateServiceOrder_WithValidData_ReturnsDtoWithStatusReceived()
    {
        var customer = ValidCustomer();
        var vehicle = ValidVehicle(customer.Id);

        var customerRepo = new FakeCustomerRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var frontDeskEmployee = await SeedFrontDeskEmployeeAsync(employeeRepo);

        await customerRepo.AddAsync(customer);
        await vehicleRepo.AddAsync(vehicle);

        var handler = new CreateServiceOrderHandler(serviceOrderRepo, customerRepo, vehicleRepo, employeeRepo);
        var command = new CreateServiceOrderCommand(customer.Id, vehicle.Id, frontDeskEmployee.Id);

        var dto = await handler.HandleAsync(command);

        dto.Should().NotBeNull();
        dto.Id.Should().NotBeEmpty();
        dto.CustomerId.Should().Be(customer.Id);
        dto.VehicleId.Should().Be(vehicle.Id);
        dto.Status.Should().Be(AppServiceOrderStatus.Received);
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateServiceOrder_WithNonExistentCustomer_ThrowsEntityNotFoundException()
    {
        var customerRepo = new FakeCustomerRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var frontDeskEmployee = await SeedFrontDeskEmployeeAsync(employeeRepo);

        var handler = new CreateServiceOrderHandler(serviceOrderRepo, customerRepo, vehicleRepo, employeeRepo);
        var command = new CreateServiceOrderCommand(Guid.NewGuid(), Guid.NewGuid(), frontDeskEmployee.Id);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateServiceOrder_WithEmptyCustomerId_ThrowsDomainException()
    {
        var customerRepo = new FakeCustomerRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var frontDeskEmployee = await SeedFrontDeskEmployeeAsync(employeeRepo);

        var handler = new CreateServiceOrderHandler(serviceOrderRepo, customerRepo, vehicleRepo, employeeRepo);
        var command = new CreateServiceOrderCommand(Guid.Empty, Guid.NewGuid(), frontDeskEmployee.Id);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Id do cliente da OS inválido");
    }

    [Fact]
    public async Task CreateServiceOrder_WithEmptyVehicleId_ThrowsDomainException()
    {
        var customerRepo = new FakeCustomerRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var frontDeskEmployee = await SeedFrontDeskEmployeeAsync(employeeRepo);

        var handler = new CreateServiceOrderHandler(serviceOrderRepo, customerRepo, vehicleRepo, employeeRepo);
        var command = new CreateServiceOrderCommand(Guid.NewGuid(), Guid.Empty, frontDeskEmployee.Id);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Id do veículo da OS inválido");
    }

    [Fact]
    public async Task CreateServiceOrder_WithNonExistentVehicle_ThrowsEntityNotFoundException()
    {
        var customer = ValidCustomer();
        var customerRepo = new FakeCustomerRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var frontDeskEmployee = await SeedFrontDeskEmployeeAsync(employeeRepo);

        await customerRepo.AddAsync(customer);

        var handler = new CreateServiceOrderHandler(serviceOrderRepo, customerRepo, vehicleRepo, employeeRepo);
        var command = new CreateServiceOrderCommand(customer.Id, Guid.NewGuid(), frontDeskEmployee.Id);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateServiceOrder_WithVehicleFromAnotherCustomer_ThrowsDomainException()
    {
        var customer1 = ValidCustomer();
        var customer2 = Customer.Create(
            "Maria Souza", CustomerDocumentType.Cpf, "111.444.777-35",
            "maria@email.com", "11911112222", ValidAddress());
        var vehicleFromCustomer1 = ValidVehicle(customer1.Id);

        var customerRepo = new FakeCustomerRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var frontDeskEmployee = await SeedFrontDeskEmployeeAsync(employeeRepo);

        await customerRepo.AddAsync(customer1);
        await customerRepo.AddAsync(customer2);
        await vehicleRepo.AddAsync(vehicleFromCustomer1);

        var handler = new CreateServiceOrderHandler(serviceOrderRepo, customerRepo, vehicleRepo, employeeRepo);
        var command = new CreateServiceOrderCommand(customer2.Id, vehicleFromCustomer1.Id, frontDeskEmployee.Id);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Veículo não pertence ao cliente informado para a OS");
    }

    // GetById handler tests

    [Fact]
    public async Task GetServiceOrderById_Existing_ReturnsDto()
    {
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var repo = new FakeServiceOrderRepository();
        await repo.AddAsync(serviceOrder);

        var handler = new GetServiceOrderByIdHandler(repo);
        var dto = await handler.HandleAsync(new GetServiceOrderByIdQuery(serviceOrder.Id));

        dto.Should().NotBeNull();
        dto.Id.Should().Be(serviceOrder.Id);
        dto.Status.Should().Be(AppServiceOrderStatus.Received);
    }

    [Fact]
    public async Task GetServiceOrderById_NotFound_ThrowsEntityNotFoundException()
    {
        var repo = new FakeServiceOrderRepository();
        var handler = new GetServiceOrderByIdHandler(repo);

        var act = async () => await handler.HandleAsync(new GetServiceOrderByIdQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // List handler tests

    [Fact]
    public async Task ListServiceOrders_ReturnsPaged()
    {
        var repo = new FakeServiceOrderRepository();
        await repo.AddAsync(ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        await repo.AddAsync(ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        var handler = new ListServiceOrdersHandler(repo);
        var result = await handler.HandleAsync(new ListServiceOrdersQuery(1, 10));

        result.Should().NotBeNull();
        result.Items.Count.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task ListServiceOrders_EmptyRepository_ReturnsEmptyPaged()
    {
        var repo = new FakeServiceOrderRepository();
        var handler = new ListServiceOrdersHandler(repo);

        var result = await handler.HandleAsync(new ListServiceOrdersQuery(1, 20));

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
