using FluentAssertions;
using GarageFlow.Application.Services.Commands;
using GarageFlow.Application.Services.Handlers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Supplies;
using GarageFlow.Tests.Application.Supplies;
using AppSupplyUnit = GarageFlow.Application.Services.Enums.SupplyUnit;

namespace GarageFlow.Tests.Application.Services;

public sealed class AddServiceSupplyHandlerTests
{
    private static async Task<(FakeServiceRepository serviceRepo, FakeSupplyRepository supplyRepo, Guid serviceId, Guid supplyId)> SetupAsync()
    {
        var serviceRepo = new FakeServiceRepository();
        var supplyRepo = new FakeSupplyRepository();

        var createServiceHandler = new CreateServiceHandler(serviceRepo);
        var serviceDto = await createServiceHandler.HandleAsync(
            new CreateServiceCommand("SRV-CSH-001", "Serviço Composição Insumo Handler", null, 100.00m, null));

        var supply = Supply.Create("Óleo Motor", "SUP-CSH-001", "L", 45.00m);
        await supplyRepo.AddAsync(supply);

        return (serviceRepo, supplyRepo, serviceDto.Id, supply.Id);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsServiceDtoWithSupply()
    {
        var (serviceRepo, supplyRepo, serviceId, supplyId) = await SetupAsync();
        var handler = new AddServiceSupplyHandler(serviceRepo, supplyRepo);

        var dto = await handler.HandleAsync(new AddServiceSupplyCommand(serviceId, supplyId, 1.5m));

        dto.Supplies.Should().HaveCount(1);
        dto.Supplies[0].SupplyId.Should().Be(supplyId);
        dto.Supplies[0].SupplyName.Should().Be("Óleo Motor");
        dto.Supplies[0].Quantity.Should().Be(1.5m);
        dto.Supplies[0].Unit.Should().Be(AppSupplyUnit.Liter);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ThrowsEntityNotFoundException()
    {
        var (_, supplyRepo, _, supplyId) = await SetupAsync();
        var serviceRepo = new FakeServiceRepository();
        var handler = new AddServiceSupplyHandler(serviceRepo, supplyRepo);

        var act = async () => await handler.HandleAsync(
            new AddServiceSupplyCommand(Guid.NewGuid(), supplyId, 1m));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentSupply_ThrowsEntityNotFoundException()
    {
        var (serviceRepo, _, serviceId, _) = await SetupAsync();
        var supplyRepo = new FakeSupplyRepository();
        var handler = new AddServiceSupplyHandler(serviceRepo, supplyRepo);

        var act = async () => await handler.HandleAsync(
            new AddServiceSupplyCommand(serviceId, Guid.NewGuid(), 1m));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithInvalidQuantity_ThrowsDomainException()
    {
        var (serviceRepo, supplyRepo, serviceId, supplyId) = await SetupAsync();
        var handler = new AddServiceSupplyHandler(serviceRepo, supplyRepo);

        var act = async () => await handler.HandleAsync(
            new AddServiceSupplyCommand(serviceId, supplyId, 0));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_WithDuplicateSupply_ThrowsDuplicateServiceSupplyException()
    {
        var (serviceRepo, supplyRepo, serviceId, supplyId) = await SetupAsync();
        var handler = new AddServiceSupplyHandler(serviceRepo, supplyRepo);

        await handler.HandleAsync(new AddServiceSupplyCommand(serviceId, supplyId, 1m));

        var act = async () => await handler.HandleAsync(
            new AddServiceSupplyCommand(serviceId, supplyId, 2m));

        await act.Should().ThrowAsync<DuplicateServiceSupplyException>();
    }

    [Fact]
    public async Task Handle_WithNonCanonicalUnit_ThrowsDomainException()
    {
        var serviceRepo = new FakeServiceRepository();
        var supplyRepo = new FakeSupplyRepository();

        var createServiceHandler = new CreateServiceHandler(serviceRepo);
        var serviceDto = await createServiceHandler.HandleAsync(
            new CreateServiceCommand("SRV-NCU-001", "Serviço Unidade Inválida", null, 100.00m, null));

        // "M" (metro) is not a canonical SupplyUnit
        var supply = Supply.Create("Cabo de Aço", "SUP-NCU-001", "M", 12.00m);
        await supplyRepo.AddAsync(supply);

        var handler = new AddServiceSupplyHandler(serviceRepo, supplyRepo);

        var act = async () => await handler.HandleAsync(
            new AddServiceSupplyCommand(serviceDto.Id, supply.Id, 1m));

        await act.Should().ThrowAsync<DomainException>();
    }
}

public sealed class RemoveServiceSupplyHandlerTests
{
    private static async Task<(FakeServiceRepository serviceRepo, Guid serviceId, Guid supplyId)> SetupWithSupplyAsync()
    {
        var serviceRepo = new FakeServiceRepository();
        var supplyRepo = new FakeSupplyRepository();

        var createServiceHandler = new CreateServiceHandler(serviceRepo);
        var serviceDto = await createServiceHandler.HandleAsync(
            new CreateServiceCommand("SRV-RSUP-001", "Serviço Remoção Insumo Handler", null, 100.00m, null));

        var supply = Supply.Create("Fluido de Freio", "SUP-RSUP-001", "ML", 8.00m);
        await supplyRepo.AddAsync(supply);

        var addHandler = new AddServiceSupplyHandler(serviceRepo, supplyRepo);
        await addHandler.HandleAsync(new AddServiceSupplyCommand(serviceDto.Id, supply.Id, 500m));

        return (serviceRepo, serviceDto.Id, supply.Id);
    }

    [Fact]
    public async Task Handle_WithLinkedSupply_RemovesSupply()
    {
        var (serviceRepo, serviceId, supplyId) = await SetupWithSupplyAsync();
        var handler = new RemoveServiceSupplyHandler(serviceRepo);

        await handler.HandleAsync(new RemoveServiceSupplyCommand(serviceId, supplyId));

        var service = await serviceRepo.GetByIdAsync(serviceId);
        service!.Supplies.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ThrowsEntityNotFoundException()
    {
        var serviceRepo = new FakeServiceRepository();
        var handler = new RemoveServiceSupplyHandler(serviceRepo);

        var act = async () => await handler.HandleAsync(
            new RemoveServiceSupplyCommand(Guid.NewGuid(), Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithUnlinkedSupply_ThrowsEntityNotFoundException()
    {
        var (serviceRepo, serviceId, _) = await SetupWithSupplyAsync();
        var handler = new RemoveServiceSupplyHandler(serviceRepo);

        var act = async () => await handler.HandleAsync(
            new RemoveServiceSupplyCommand(serviceId, Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
