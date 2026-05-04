using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Domain.ServiceOrders;

public sealed class DiagnosticTests
{
    // ── Start ──────────────────────────────────────────────────────────────

    [Fact]
    public void Start_WithValidIds_ReturnsDiagnosticInProgress()
    {
        var serviceOrderId = Guid.NewGuid();
        var mechanicId = Guid.NewGuid();

        var diagnostic = Diagnostic.Start(serviceOrderId, mechanicId);

        diagnostic.Id.Should().NotBeEmpty();
        diagnostic.ServiceOrderId.Should().Be(serviceOrderId);
        diagnostic.MechanicId.Should().Be(mechanicId);
        diagnostic.Status.Should().Be(DiagnosticStatus.InProgress);
        diagnostic.SelectedServices.Should().BeEmpty();
        diagnostic.Description.Should().BeNull();
        diagnostic.CompletedAt.Should().BeNull();
        diagnostic.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Start_WithEmptyServiceOrderId_ThrowsDomainException()
    {
        var act = () => Diagnostic.Start(Guid.Empty, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidServiceOrderId);
    }

    [Fact]
    public void Start_WithEmptyMechanicId_ThrowsDomainException()
    {
        var act = () => Diagnostic.Start(Guid.NewGuid(), Guid.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidDiagnosticMechanicId);
    }

    // ── AddService ─────────────────────────────────────────────────────────

    [Fact]
    public void AddService_WithInProgressStatus_AddsServiceId()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        var serviceId = Guid.NewGuid();

        diagnostic.AddService(serviceId);

        diagnostic.SelectedServices.Should().ContainSingle(id => id == serviceId);
    }

    [Fact]
    public void AddService_WhenCompleted_ThrowsDiagnosticNotInProgressException()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        var serviceId = Guid.NewGuid();
        diagnostic.AddService(serviceId);
        diagnostic.Complete("Diagnóstico finalizado.");

        var act = () => diagnostic.AddService(Guid.NewGuid());

        act.Should().Throw<DiagnosticNotInProgressException>()
            .WithMessage(DomainErrorMessages.DiagnosticAlreadyCompleted);
    }

    [Fact]
    public void AddService_DuplicateServiceId_ThrowsDuplicateDiagnosticServiceException()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        var serviceId = Guid.NewGuid();
        diagnostic.AddService(serviceId);

        var act = () => diagnostic.AddService(serviceId);

        act.Should().Throw<DuplicateDiagnosticServiceException>()
            .WithMessage(DomainErrorMessages.DiagnosticServiceAlreadyAdded);
    }

    // ── RemoveService ──────────────────────────────────────────────────────

    [Fact]
    public void RemoveService_WithTwoServices_RemovesCorrectOne()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();
        diagnostic.AddService(serviceId1);
        diagnostic.AddService(serviceId2);

        diagnostic.RemoveService(serviceId1);

        diagnostic.SelectedServices.Should().ContainSingle(id => id == serviceId2);
        diagnostic.SelectedServices.Should().NotContain(serviceId1);
    }

    [Fact]
    public void RemoveService_WhenCompleted_ThrowsDiagnosticNotInProgressException()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        var serviceId = Guid.NewGuid();
        diagnostic.AddService(serviceId);
        diagnostic.Complete("Diagnóstico finalizado.");

        var act = () => diagnostic.RemoveService(serviceId);

        act.Should().Throw<DiagnosticNotInProgressException>()
            .WithMessage(DomainErrorMessages.DiagnosticAlreadyCompleted);
    }

    [Fact]
    public void RemoveService_ServiceNotInDiagnostic_ThrowsEntityNotFoundException()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        var serviceId = Guid.NewGuid();
        diagnostic.AddService(serviceId);

        var act = () => diagnostic.RemoveService(Guid.NewGuid());

        act.Should().Throw<EntityNotFoundException>();
    }

    [Fact]
    public void RemoveService_OnlyService_ThrowsDiagnosticLastServiceException()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        var serviceId = Guid.NewGuid();
        diagnostic.AddService(serviceId);

        var act = () => diagnostic.RemoveService(serviceId);

        act.Should().Throw<DiagnosticLastServiceException>()
            .WithMessage(DomainErrorMessages.DiagnosticMustHaveAtLeastOneService);
    }

    // ── Complete ───────────────────────────────────────────────────────────

    [Fact]
    public void Complete_WithValidData_SetsDiagnosticToCompleted()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        diagnostic.AddService(Guid.NewGuid());

        diagnostic.Complete("Motor com desgaste nas buchas.");

        diagnostic.Status.Should().Be(DiagnosticStatus.Completed);
        diagnostic.Description.Should().Be("Motor com desgaste nas buchas.");
        diagnostic.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ThrowsDiagnosticNotInProgressException()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        diagnostic.AddService(Guid.NewGuid());
        diagnostic.Complete("Primeira conclusão.");

        var act = () => diagnostic.Complete("Tentativa de reabrir.");

        act.Should().Throw<DiagnosticNotInProgressException>()
            .WithMessage(DomainErrorMessages.DiagnosticAlreadyCompleted);
    }

    [Fact]
    public void Complete_WithEmptyDescription_ThrowsDomainException()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        diagnostic.AddService(Guid.NewGuid());

        var act = () => diagnostic.Complete(string.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.DiagnosticDescriptionRequired);
    }

    [Fact]
    public void Complete_WithWhitespaceDescription_ThrowsDomainException()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());
        diagnostic.AddService(Guid.NewGuid());

        var act = () => diagnostic.Complete("   ");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.DiagnosticDescriptionRequired);
    }

    [Fact]
    public void Complete_WithNoServices_ThrowsDiagnosticNoServicesException()
    {
        var diagnostic = Diagnostic.Start(Guid.NewGuid(), Guid.NewGuid());

        var act = () => diagnostic.Complete("Diagnóstico sem serviços.");

        act.Should().Throw<DiagnosticNoServicesException>()
            .WithMessage(DomainErrorMessages.DiagnosticMustHaveAtLeastOneService);
    }

    // ── ServiceOrder.StartDiagnostic ───────────────────────────────────────

    [Fact]
    public void ServiceOrder_StartDiagnostic_WithValidMechanic_SetsDiagnosticAndStatus()
    {
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var mechanicId = Guid.NewGuid();

        serviceOrder.StartDiagnostic(mechanicId);

        serviceOrder.Diagnostic.Should().NotBeNull();
        serviceOrder.Diagnostic!.MechanicId.Should().Be(mechanicId);
        serviceOrder.Diagnostic.Status.Should().Be(DiagnosticStatus.InProgress);
        serviceOrder.Status.Should().Be(ServiceOrderStatus.InDiagnostic);
        serviceOrder.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ServiceOrder_StartDiagnostic_WhenAlreadyInDiagnostic_ThrowsDiagnosticAlreadyStartedException()
    {
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnostic(Guid.NewGuid());

        var act = () => serviceOrder.StartDiagnostic(Guid.NewGuid());

        act.Should().Throw<DiagnosticAlreadyStartedException>()
            .WithMessage(DomainErrorMessages.DiagnosticAlreadyStarted);
    }

    [Fact]
    public void ServiceOrder_StartDiagnostic_WithEmptyMechanicId_ThrowsDomainException()
    {
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => serviceOrder.StartDiagnostic(Guid.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.InvalidDiagnosticMechanicId);
    }

    [Fact]
    public void ServiceOrder_AddDiagnosticService_WhenNoDiagnostic_ThrowsDiagnosticNotInProgressException()
    {
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => serviceOrder.AddDiagnosticService(Guid.NewGuid());

        act.Should().Throw<DiagnosticNotInProgressException>()
            .WithMessage(DomainErrorMessages.DiagnosticNotStarted);
    }
}
