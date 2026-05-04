using FluentAssertions;
using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.Handlers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Services;
using GarageFlow.Tests.Application.Services;

namespace GarageFlow.Tests.Application.ServiceOrders;

public sealed class DiagnosticHandlersTests
{
    private static Service ValidService() =>
        Service.Create("SVC-DIAG-001", "Troca de Óleo Diagnóstico", null, 80m, 60);

    // ── StartDiagnosticHandler ─────────────────────────────────────────────

    [Fact]
    public async Task StartDiagnostic_WithValidData_ReturnsUpdatedDtoWithDiagnostic()
    {
        var repo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await repo.AddAsync(serviceOrder);

        var handler = new StartDiagnosticHandler(repo);
        var mechanicId = Guid.NewGuid();
        var command = new StartDiagnosticCommand(serviceOrder.Id, mechanicId);

        var dto = await handler.HandleAsync(command);

        dto.Diagnostic.Should().NotBeNull();
        dto.Diagnostic!.MechanicId.Should().Be(mechanicId);
        dto.Diagnostic.Status.Should().Be(DiagnosticStatus.InProgress);
        dto.Status.Should().Be(ServiceOrderStatus.InDiagnostic);
    }

    [Fact]
    public async Task StartDiagnostic_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var repo = new FakeServiceOrderRepository();
        var handler = new StartDiagnosticHandler(repo);
        var command = new StartDiagnosticCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task StartDiagnostic_WithEmptyMechanicId_ThrowsDomainException()
    {
        var repo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await repo.AddAsync(serviceOrder);

        var handler = new StartDiagnosticHandler(repo);
        var command = new StartDiagnosticCommand(serviceOrder.Id, Guid.Empty);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task StartDiagnostic_WhenAlreadyStarted_ThrowsDiagnosticAlreadyStartedException()
    {
        var repo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        await repo.AddAsync(serviceOrder);

        var handler = new StartDiagnosticHandler(repo);
        var command = new StartDiagnosticCommand(serviceOrder.Id, Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DiagnosticAlreadyStartedException>();
    }

    // ── AddDiagnosticServiceHandler ────────────────────────────────────────

    [Fact]
    public async Task AddDiagnosticService_WithValidData_ReturnsDtoWithService()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        await soRepo.AddAsync(serviceOrder);

        var service = ValidService();
        await svcRepo.AddAsync(service);

        var handler = new AddDiagnosticServiceHandler(soRepo, svcRepo);
        var command = new AddDiagnosticServiceCommand(serviceOrder.Id, service.Id);

        var dto = await handler.HandleAsync(command);

        dto.Diagnostic!.SelectedServices.Should().ContainSingle(id => id == service.Id);
    }

    [Fact]
    public async Task AddDiagnosticService_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();

        var handler = new AddDiagnosticServiceHandler(soRepo, svcRepo);
        var command = new AddDiagnosticServiceCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddDiagnosticService_WithNonExistentService_ThrowsEntityNotFoundException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        await soRepo.AddAsync(serviceOrder);

        var handler = new AddDiagnosticServiceHandler(soRepo, svcRepo);
        var command = new AddDiagnosticServiceCommand(serviceOrder.Id, Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddDiagnosticService_WhenDiagnosticNotStarted_ThrowsDiagnosticNotInProgressException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await soRepo.AddAsync(serviceOrder);

        var service = ValidService();
        await svcRepo.AddAsync(service);

        var handler = new AddDiagnosticServiceHandler(soRepo, svcRepo);
        var command = new AddDiagnosticServiceCommand(serviceOrder.Id, service.Id);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DiagnosticNotInProgressException>();
    }

    [Fact]
    public async Task AddDiagnosticService_DuplicateService_ThrowsDuplicateDiagnosticServiceException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        await soRepo.AddAsync(serviceOrder);

        var service = ValidService();
        await svcRepo.AddAsync(service);

        var handler = new AddDiagnosticServiceHandler(soRepo, svcRepo);
        var command = new AddDiagnosticServiceCommand(serviceOrder.Id, service.Id);
        await handler.HandleAsync(command);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DuplicateDiagnosticServiceException>();
    }

    // ── RemoveDiagnosticServiceHandler ─────────────────────────────────────

    [Fact]
    public async Task RemoveDiagnosticService_WithTwoServices_RemovesSuccessfully()
    {
        var soRepo = new FakeServiceOrderRepository();
        var svcRepo = new FakeServiceRepository();

        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();
        serviceOrder.AddDiagnosticService(serviceId1);
        serviceOrder.AddDiagnosticService(serviceId2);
        await soRepo.AddAsync(serviceOrder);

        var handler = new RemoveDiagnosticServiceHandler(soRepo);
        var command = new RemoveDiagnosticServiceCommand(serviceOrder.Id, serviceId1);

        await handler.HandleAsync(command);

        serviceOrder.Diagnostic!.SelectedServices.Should().NotContain(serviceId1);
        serviceOrder.Diagnostic.SelectedServices.Should().Contain(serviceId2);
    }

    [Fact]
    public async Task RemoveDiagnosticService_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var handler = new RemoveDiagnosticServiceHandler(soRepo);
        var command = new RemoveDiagnosticServiceCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task RemoveDiagnosticService_WhenOnlyService_ThrowsDiagnosticLastServiceException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        var serviceId = Guid.NewGuid();
        serviceOrder.AddDiagnosticService(serviceId);
        await soRepo.AddAsync(serviceOrder);

        var handler = new RemoveDiagnosticServiceHandler(soRepo);
        var command = new RemoveDiagnosticServiceCommand(serviceOrder.Id, serviceId);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DiagnosticLastServiceException>();
    }

    // ── CompleteDiagnosticHandler ──────────────────────────────────────────

    [Fact]
    public async Task CompleteDiagnostic_WithValidData_ReturnsDtoWithCompletedDiagnostic()
    {
        var soRepo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        serviceOrder.AddDiagnosticService(Guid.NewGuid());
        await soRepo.AddAsync(serviceOrder);

        var handler = new CompleteDiagnosticHandler(soRepo);
        var command = new CompleteDiagnosticCommand(serviceOrder.Id, "Motor com desgaste nas buchas.");

        var dto = await handler.HandleAsync(command);

        dto.Diagnostic!.Status.Should().Be(DiagnosticStatus.Completed);
        dto.Diagnostic.Description.Should().Be("Motor com desgaste nas buchas.");
        dto.Diagnostic.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteDiagnostic_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var handler = new CompleteDiagnosticHandler(soRepo);
        var command = new CompleteDiagnosticCommand(Guid.NewGuid(), "Descrição");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CompleteDiagnostic_WithEmptyDescription_ThrowsDomainException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        serviceOrder.AddDiagnosticService(Guid.NewGuid());
        await soRepo.AddAsync(serviceOrder);

        var handler = new CompleteDiagnosticHandler(soRepo);
        var command = new CompleteDiagnosticCommand(serviceOrder.Id, string.Empty);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task CompleteDiagnostic_WithNoServices_ThrowsDiagnosticNoServicesException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        await soRepo.AddAsync(serviceOrder);

        var handler = new CompleteDiagnosticHandler(soRepo);
        var command = new CompleteDiagnosticCommand(serviceOrder.Id, "Diagnóstico sem serviços.");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DiagnosticNoServicesException>();
    }

    [Fact]
    public async Task CompleteDiagnostic_WhenAlreadyCompleted_ThrowsDiagnosticNotInProgressException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        serviceOrder.AddDiagnosticService(Guid.NewGuid());
        serviceOrder.CompleteDiagnostic("Primeira conclusão.");
        await soRepo.AddAsync(serviceOrder);

        var handler = new CompleteDiagnosticHandler(soRepo);
        var command = new CompleteDiagnosticCommand(serviceOrder.Id, "Segunda tentativa.");

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DiagnosticNotInProgressException>();
    }

    // ── ConsolidateDiagnosticServicesHandler ───────────────────────────────

    [Fact]
    public async Task ConsolidateDiagnosticServices_WithCompletedDiagnostic_ReturnsUpdatedDto()
    {
        var soRepo = new FakeServiceOrderRepository();
        var serviceId = Guid.NewGuid();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        serviceOrder.AddDiagnosticService(serviceId);
        serviceOrder.CompleteDiagnostic("Diagnóstico concluído.");
        await soRepo.AddAsync(serviceOrder);

        var handler = new ConsolidateDiagnosticServicesHandler(soRepo);
        var command = new ConsolidateDiagnosticServicesCommand(serviceOrder.Id);

        var dto = await handler.HandleAsync(command);

        dto.Services.Should().ContainSingle(s => s.ServiceId == serviceId && s.IsActive);
        dto.ServiceHistory.Should().ContainSingle(h =>
            h.ServiceId == serviceId &&
            h.Action == ServiceOrderServiceAction.Added &&
            h.Source == ServiceSource.Diagnostic);
    }

    [Fact]
    public async Task ConsolidateDiagnosticServices_WithNonExistentServiceOrder_ThrowsEntityNotFoundException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var handler = new ConsolidateDiagnosticServicesHandler(soRepo);
        var command = new ConsolidateDiagnosticServicesCommand(Guid.NewGuid());

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ConsolidateDiagnosticServices_WithNoDiagnostic_ThrowsDiagnosticNotCompletedException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await soRepo.AddAsync(serviceOrder);

        var handler = new ConsolidateDiagnosticServicesHandler(soRepo);
        var command = new ConsolidateDiagnosticServicesCommand(serviceOrder.Id);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DiagnosticNotCompletedException>();
    }

    [Fact]
    public async Task ConsolidateDiagnosticServices_WithInProgressDiagnostic_ThrowsDiagnosticNotCompletedException()
    {
        var soRepo = new FakeServiceOrderRepository();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        serviceOrder.AddDiagnosticService(Guid.NewGuid());
        await soRepo.AddAsync(serviceOrder);

        var handler = new ConsolidateDiagnosticServicesHandler(soRepo);
        var command = new ConsolidateDiagnosticServicesCommand(serviceOrder.Id);

        var act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<DiagnosticNotCompletedException>();
    }
}
