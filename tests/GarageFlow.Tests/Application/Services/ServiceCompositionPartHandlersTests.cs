using FluentAssertions;
using GarageFlow.Application.Services.Commands;
using GarageFlow.Application.Services.Handlers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Tests.Application.Parts;

namespace GarageFlow.Tests.Application.Services;

public sealed class AddServicePartHandlerTests
{
    private static async Task<(FakeServiceRepository serviceRepo, FakePartRepository partRepo, Guid serviceId, Guid partId)> SetupAsync()
    {
        var serviceRepo = new FakeServiceRepository();
        var partRepo = new FakePartRepository();

        var createServiceHandler = new CreateServiceHandler(serviceRepo);
        var serviceDto = await createServiceHandler.HandleAsync(
            new CreateServiceCommand("SRV-CPH-001", "Serviço Composição Handler", null, 100.00m, null));

        var part = Part.Create("Filtro de Óleo", "PART-CPH-001", "SKU-CPH-001", "UN", 25.00m);
        await partRepo.AddAsync(part);

        return (serviceRepo, partRepo, serviceDto.Id, part.Id);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsServiceDtoWithPart()
    {
        var (serviceRepo, partRepo, serviceId, partId) = await SetupAsync();
        var handler = new AddServicePartHandler(serviceRepo, partRepo);

        var dto = await handler.HandleAsync(new AddServicePartCommand(serviceId, partId, 2));

        dto.Parts.Should().HaveCount(1);
        dto.Parts[0].PartId.Should().Be(partId);
        dto.Parts[0].PartName.Should().Be("Filtro de Óleo");
        dto.Parts[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ThrowsEntityNotFoundException()
    {
        var (_, partRepo, _, partId) = await SetupAsync();
        var serviceRepo = new FakeServiceRepository();
        var handler = new AddServicePartHandler(serviceRepo, partRepo);

        var act = async () => await handler.HandleAsync(
            new AddServicePartCommand(Guid.NewGuid(), partId, 2));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentPart_ThrowsEntityNotFoundException()
    {
        var (serviceRepo, _, serviceId, _) = await SetupAsync();
        var partRepo = new FakePartRepository();
        var handler = new AddServicePartHandler(serviceRepo, partRepo);

        var act = async () => await handler.HandleAsync(
            new AddServicePartCommand(serviceId, Guid.NewGuid(), 2));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithInvalidQuantity_ThrowsDomainException()
    {
        var (serviceRepo, partRepo, serviceId, partId) = await SetupAsync();
        var handler = new AddServicePartHandler(serviceRepo, partRepo);

        var act = async () => await handler.HandleAsync(
            new AddServicePartCommand(serviceId, partId, 0));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_WithDuplicatePart_ThrowsDuplicateServicePartException()
    {
        var (serviceRepo, partRepo, serviceId, partId) = await SetupAsync();
        var handler = new AddServicePartHandler(serviceRepo, partRepo);

        await handler.HandleAsync(new AddServicePartCommand(serviceId, partId, 1));

        var act = async () => await handler.HandleAsync(
            new AddServicePartCommand(serviceId, partId, 2));

        await act.Should().ThrowAsync<DuplicateServicePartException>();
    }
}

public sealed class RemoveServicePartHandlerTests
{
    private static async Task<(FakeServiceRepository serviceRepo, Guid serviceId, Guid partId)> SetupWithPartAsync()
    {
        var serviceRepo = new FakeServiceRepository();
        var partRepo = new FakePartRepository();

        var createServiceHandler = new CreateServiceHandler(serviceRepo);
        var serviceDto = await createServiceHandler.HandleAsync(
            new CreateServiceCommand("SRV-REM-001", "Serviço Remoção Handler", null, 100.00m, null));

        var part = Part.Create("Correia Dentada", "PART-REM-001", "SKU-REM-001", "UN", 40.00m);
        await partRepo.AddAsync(part);

        var addHandler = new AddServicePartHandler(serviceRepo, partRepo);
        await addHandler.HandleAsync(new AddServicePartCommand(serviceDto.Id, part.Id, 1));

        return (serviceRepo, serviceDto.Id, part.Id);
    }

    [Fact]
    public async Task Handle_WithLinkedPart_RemovesPart()
    {
        var (serviceRepo, serviceId, partId) = await SetupWithPartAsync();
        var handler = new RemoveServicePartHandler(serviceRepo);

        await handler.HandleAsync(new RemoveServicePartCommand(serviceId, partId));

        var service = await serviceRepo.GetByIdAsync(serviceId);
        service!.Parts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ThrowsEntityNotFoundException()
    {
        var serviceRepo = new FakeServiceRepository();
        var handler = new RemoveServicePartHandler(serviceRepo);

        var act = async () => await handler.HandleAsync(
            new RemoveServicePartCommand(Guid.NewGuid(), Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithUnlinkedPart_ThrowsEntityNotFoundException()
    {
        var (serviceRepo, serviceId, _) = await SetupWithPartAsync();
        var handler = new RemoveServicePartHandler(serviceRepo);

        var act = async () => await handler.HandleAsync(
            new RemoveServicePartCommand(serviceId, Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
