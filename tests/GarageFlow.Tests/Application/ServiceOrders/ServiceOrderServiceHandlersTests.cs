using FluentAssertions;
using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.Handlers;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Supplies;
using GarageFlow.Domain.ValueObjects;
using GarageFlow.Tests.Application.Services;

namespace GarageFlow.Tests.Application.ServiceOrders;

public sealed class ServiceOrderServiceHandlersTests
{
    private static Address ValidAddress() => Address.Create(
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

    private static Customer ValidCustomer() => Customer.Create(
        "João Silva", CustomerDocumentType.Cpf, "529.982.247-25",
        "joao@email.com", "11987654321", ValidAddress());

    private static Service ValidService() => Service.Create(
        "SVC-TST-001", "Troca de Óleo", null, 80m, 60);

    // AddServiceToServiceOrder tests

    [Fact]
    public async Task AddService_WithValidData_ReturnsUpdatedDto()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var serviceRepo = new FakeServiceRepository();

        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var serviceOrder = ServiceOrder.Create(customerId, vehicleId);
        await serviceOrderRepo.AddAsync(serviceOrder);

        var service = ValidService();
        await serviceRepo.AddAsync(service);

        var handler = new AddServiceToServiceOrderHandler(serviceOrderRepo, serviceRepo);
        var command = new AddServiceToServiceOrderCommand(serviceOrder.Id, service.Id, Guid.NewGuid());

        var dto = await handler.HandleAsync(command);

        dto.Services.Should().HaveCount(1);
        dto.Services.Single().ServiceId.Should().Be(service.Id);
        dto.ServiceHistory.Should().HaveCount(1);
        dto.ServiceHistory.Single().Action.Should().Be(ServiceOrderServiceAction.Added);
    }

    [Fact]
    public async Task AddService_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var serviceRepo = new FakeServiceRepository();

        var handler = new AddServiceToServiceOrderHandler(serviceOrderRepo, serviceRepo);
        var command = new AddServiceToServiceOrderCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddService_WithNonExistentService_ThrowsEntityNotFoundException()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var serviceRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await serviceOrderRepo.AddAsync(serviceOrder);

        var handler = new AddServiceToServiceOrderHandler(serviceOrderRepo, serviceRepo);
        var command = new AddServiceToServiceOrderCommand(serviceOrder.Id, Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddService_WithInactiveService_ThrowsDomainException()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var serviceRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await serviceOrderRepo.AddAsync(serviceOrder);

        var service = ValidService();
        await serviceRepo.AddAsync(service);
        service.Deactivate();

        var handler = new AddServiceToServiceOrderHandler(serviceOrderRepo, serviceRepo);
        var command = new AddServiceToServiceOrderCommand(serviceOrder.Id, service.Id, Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task AddService_WithEmptyActorId_ThrowsDomainException()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var serviceRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await serviceOrderRepo.AddAsync(serviceOrder);

        var handler = new AddServiceToServiceOrderHandler(serviceOrderRepo, serviceRepo);
        var command = new AddServiceToServiceOrderCommand(serviceOrder.Id, Guid.NewGuid(), Guid.Empty);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task AddService_DuplicateActive_ThrowsDuplicateServiceOrderServiceException()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var serviceRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await serviceOrderRepo.AddAsync(serviceOrder);

        var service = ValidService();
        await serviceRepo.AddAsync(service);

        var handler = new AddServiceToServiceOrderHandler(serviceOrderRepo, serviceRepo);
        var command = new AddServiceToServiceOrderCommand(serviceOrder.Id, service.Id, Guid.NewGuid());

        await handler.HandleAsync(command);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DuplicateServiceOrderServiceException>();
    }

    // RemoveServiceFromServiceOrder tests

    [Fact]
    public async Task RemoveService_WithValidData_RemovesServiceFromOrder()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();
        var serviceRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await serviceOrderRepo.AddAsync(serviceOrder);

        var service = ValidService();
        await serviceRepo.AddAsync(service);

        var addHandler = new AddServiceToServiceOrderHandler(serviceOrderRepo, serviceRepo);
        await addHandler.HandleAsync(new AddServiceToServiceOrderCommand(serviceOrder.Id, service.Id, Guid.NewGuid()));

        var removeHandler = new RemoveServiceFromServiceOrderHandler(serviceOrderRepo);
        var command = new RemoveServiceFromServiceOrderCommand(serviceOrder.Id, service.Id, Guid.NewGuid(), "Cliente solicitou remoção");

        await removeHandler.HandleAsync(command);

        serviceOrder.Services.Single().IsActive.Should().BeFalse();
        serviceOrder.ServiceHistory.Should().HaveCount(2);
    }

    [Fact]
    public async Task RemoveService_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();

        var handler = new RemoveServiceFromServiceOrderHandler(serviceOrderRepo);
        var command = new RemoveServiceFromServiceOrderCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "motivo");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task RemoveService_WithEmptyReason_ThrowsDomainException()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await serviceOrderRepo.AddAsync(serviceOrder);

        var handler = new RemoveServiceFromServiceOrderHandler(serviceOrderRepo);
        var command = new RemoveServiceFromServiceOrderCommand(serviceOrder.Id, Guid.NewGuid(), Guid.NewGuid(), "");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task RemoveService_WithEmptyActorId_ThrowsDomainException()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await serviceOrderRepo.AddAsync(serviceOrder);

        var handler = new RemoveServiceFromServiceOrderHandler(serviceOrderRepo);
        var command = new RemoveServiceFromServiceOrderCommand(serviceOrder.Id, Guid.NewGuid(), Guid.Empty, "motivo");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task RemoveService_NotLinked_ThrowsEntityNotFoundException()
    {
        var serviceOrderRepo = new FakeServiceOrderRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await serviceOrderRepo.AddAsync(serviceOrder);

        var handler = new RemoveServiceFromServiceOrderHandler(serviceOrderRepo);
        var command = new RemoveServiceFromServiceOrderCommand(serviceOrder.Id, Guid.NewGuid(), Guid.NewGuid(), "motivo");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
