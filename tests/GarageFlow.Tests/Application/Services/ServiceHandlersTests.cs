using FluentAssertions;
using GarageFlow.Application.Services.Commands;
using GarageFlow.Application.Services.Handlers;
using GarageFlow.Application.Services.Queries;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Services;

public sealed class CreateServiceHandlerTests
{
    private static CreateServiceCommand ValidCommand(string code = "SRV-001", string name = "Troca de Óleo") => new(
        code, name, "Serviço de troca de óleo", 150.00m, 30);

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsServiceDto()
    {
        var repo = new FakeServiceRepository();
        var handler = new CreateServiceHandler(repo);

        var dto = await handler.HandleAsync(ValidCommand());

        dto.Should().NotBeNull();
        dto.Code.Should().Be("SRV-001");
        dto.Name.Should().Be("Troca de Óleo");
        dto.BasePrice.Should().Be(150.00m);
        dto.IsActive.Should().BeTrue();
        dto.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ThrowsDuplicateServiceDataException()
    {
        var repo = new FakeServiceRepository();
        var handler = new CreateServiceHandler(repo);

        await handler.HandleAsync(ValidCommand());
        var act = async () => await handler.HandleAsync(ValidCommand("SRV-001", "Outro Serviço"));

        await act.Should().ThrowAsync<DuplicateServiceDataException>()
            .WithMessage("Código do serviço já cadastrado");
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ThrowsDuplicateServiceDataException()
    {
        var repo = new FakeServiceRepository();
        var handler = new CreateServiceHandler(repo);

        await handler.HandleAsync(ValidCommand());
        var act = async () => await handler.HandleAsync(ValidCommand("SRV-002", "Troca de Óleo"));

        await act.Should().ThrowAsync<DuplicateServiceDataException>()
            .WithMessage("Nome do serviço já cadastrado");
    }
}

public sealed class GetServiceByIdHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingId_ReturnsServiceDto()
    {
        var repo = new FakeServiceRepository();
        var createHandler = new CreateServiceHandler(repo);
        var created = await createHandler.HandleAsync(new CreateServiceCommand("SRV-001", "Troca de Óleo", null, 150.00m, null));

        var getHandler = new GetServiceByIdHandler(repo);
        var dto = await getHandler.HandleAsync(new GetServiceByIdQuery(created.Id));

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(created.Id);
        dto.Code.Should().Be("SRV-001");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakeServiceRepository();
        var handler = new GetServiceByIdHandler(repo);

        var act = async () => await handler.HandleAsync(new GetServiceByIdQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}

public sealed class ListServicesHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingServices_ReturnsPaged()
    {
        var repo = new FakeServiceRepository();
        var createHandler = new CreateServiceHandler(repo);
        await createHandler.HandleAsync(new CreateServiceCommand("SRV-001", "Serviço A", null, 100.00m, null));
        await createHandler.HandleAsync(new CreateServiceCommand("SRV-002", "Serviço B", null, 200.00m, null));

        var listHandler = new ListServicesHandler(repo);
        var result = await listHandler.HandleAsync(new ListServicesQuery(1, 10));

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}

public sealed class UpdateServiceHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_UpdatesService()
    {
        var repo = new FakeServiceRepository();
        var createHandler = new CreateServiceHandler(repo);
        var created = await createHandler.HandleAsync(new CreateServiceCommand("SRV-001", "Troca de Óleo", null, 150.00m, null));

        var updateHandler = new UpdateServiceHandler(repo);
        var updated = await updateHandler.HandleAsync(
            new UpdateServiceCommand(created.Id, "Troca de Óleo Sintético", "Nova descrição", 200.00m, 45));

        updated.Name.Should().Be("Troca de Óleo Sintético");
        updated.BasePrice.Should().Be(200.00m);
        updated.Code.Should().Be("SRV-001"); // imutável
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakeServiceRepository();
        var handler = new UpdateServiceHandler(repo);

        var act = async () => await handler.HandleAsync(
            new UpdateServiceCommand(Guid.NewGuid(), "Nome", null, 100.00m, null));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDuplicateNameOnUpdate_ThrowsDuplicateServiceDataException()
    {
        var repo = new FakeServiceRepository();
        var createHandler = new CreateServiceHandler(repo);
        await createHandler.HandleAsync(new CreateServiceCommand("SRV-001", "Serviço A", null, 100.00m, null));
        var second = await createHandler.HandleAsync(new CreateServiceCommand("SRV-002", "Serviço B", null, 200.00m, null));

        var updateHandler = new UpdateServiceHandler(repo);
        var act = async () => await updateHandler.HandleAsync(
            new UpdateServiceCommand(second.Id, "Serviço A", null, 200.00m, null));

        await act.Should().ThrowAsync<DuplicateServiceDataException>()
            .WithMessage("Nome do serviço já cadastrado");
    }
}

public sealed class DeactivateServiceHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingActiveService_DeactivatesIt()
    {
        var repo = new FakeServiceRepository();
        var createHandler = new CreateServiceHandler(repo);
        var created = await createHandler.HandleAsync(new CreateServiceCommand("SRV-001", "Troca de Óleo", null, 150.00m, null));

        var deactivateHandler = new DeactivateServiceHandler(repo);
        await deactivateHandler.HandleAsync(new DeactivateServiceCommand(created.Id));

        var service = repo.All.First(s => s.Id == created.Id);
        service.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsEntityNotFoundException()
    {
        var repo = new FakeServiceRepository();
        var handler = new DeactivateServiceHandler(repo);

        var act = async () => await handler.HandleAsync(new DeactivateServiceCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
