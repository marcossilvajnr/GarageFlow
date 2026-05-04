using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Domain.ServiceOrders;

public sealed class ServiceOrderDiagnosticConsolidationTests
{
    private static ServiceOrder CreateOrderInDiagnostic(out Guid mechanicId)
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        mechanicId = Guid.NewGuid();
        order.StartDiagnostic(mechanicId);
        return order;
    }

    // ── ConsolidateDiagnosticServices ──────────────────────────────────────

    [Fact]
    public void ConsolidateDiagnosticServices_WithCompletedDiagnostic_AddsServicesToOrder()
    {
        var order = CreateOrderInDiagnostic(out var mechanicId);
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();

        order.AddDiagnosticService(serviceId1);
        order.AddDiagnosticService(serviceId2);
        order.CompleteDiagnostic("Diagnóstico concluído.");

        order.ConsolidateDiagnosticServices();

        order.Services.Should().HaveCount(2);
        order.Services.Should().AllSatisfy(s => s.IsActive.Should().BeTrue());
        order.Services.Should().AllSatisfy(s => s.Source.Should().Be(ServiceSource.Diagnostic));
        order.Services.Should().AllSatisfy(s => s.AddedByActorId.Should().Be(mechanicId));
    }

    [Fact]
    public void ConsolidateDiagnosticServices_WithCompletedDiagnostic_RegistersHistoryForEachService()
    {
        var order = CreateOrderInDiagnostic(out var mechanicId);
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();

        order.AddDiagnosticService(serviceId1);
        order.AddDiagnosticService(serviceId2);
        order.CompleteDiagnostic("Diagnóstico concluído.");

        order.ConsolidateDiagnosticServices();

        order.ServiceHistory.Should().HaveCount(2);
        order.ServiceHistory.Should().AllSatisfy(h =>
        {
            h.Action.Should().Be(ServiceOrderServiceAction.Added);
            h.Source.Should().Be(ServiceSource.Diagnostic);
            h.ActorId.Should().Be(mechanicId);
        });
    }

    [Fact]
    public void ConsolidateDiagnosticServices_WhenServiceAlreadyActiveInOrder_SkipsDuplicate()
    {
        var order = CreateOrderInDiagnostic(out var mechanicId);
        var sharedServiceId = Guid.NewGuid();
        var otherServiceId = Guid.NewGuid();

        // Service already added at FrontDesk
        order.AddService(sharedServiceId, Guid.NewGuid(), ServiceSource.FrontDesk);

        order.AddDiagnosticService(sharedServiceId);
        order.AddDiagnosticService(otherServiceId);
        order.CompleteDiagnostic("Diagnóstico concluído.");

        order.ConsolidateDiagnosticServices();

        // Only 2 total: 1 from FrontDesk + 1 new from Diagnostic (duplicate skipped)
        order.Services.Where(s => s.IsActive).Should().HaveCount(2);
        order.Services.Count(s => s.ServiceId == sharedServiceId && s.IsActive).Should().Be(1);
    }

    [Fact]
    public void ConsolidateDiagnosticServices_IsIdempotent_WhenCalledTwice()
    {
        var order = CreateOrderInDiagnostic(out _);
        var serviceId = Guid.NewGuid();

        order.AddDiagnosticService(serviceId);
        order.CompleteDiagnostic("Diagnóstico concluído.");

        order.ConsolidateDiagnosticServices();
        order.ConsolidateDiagnosticServices();

        order.Services.Count(s => s.ServiceId == serviceId && s.IsActive).Should().Be(1);
    }

    [Fact]
    public void ConsolidateDiagnosticServices_WithoutDiagnostic_ThrowsDiagnosticNotCompletedException()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => order.ConsolidateDiagnosticServices();

        act.Should().Throw<DiagnosticNotCompletedException>()
            .WithMessage(DomainErrorMessages.DiagnosticNotStarted);
    }

    [Fact]
    public void ConsolidateDiagnosticServices_WithInProgressDiagnostic_ThrowsDiagnosticNotCompletedException()
    {
        var order = CreateOrderInDiagnostic(out _);
        order.AddDiagnosticService(Guid.NewGuid());

        var act = () => order.ConsolidateDiagnosticServices();

        act.Should().Throw<DiagnosticNotCompletedException>()
            .WithMessage(DomainErrorMessages.DiagnosticNotCompleted);
    }

    [Fact]
    public void ConsolidateDiagnosticServices_PreservesExistingFrontDeskServices()
    {
        var order = CreateOrderInDiagnostic(out _);
        var frontDeskActorId = Guid.NewGuid();
        var frontDeskServiceId = Guid.NewGuid();
        var diagnosticServiceId = Guid.NewGuid();

        order.AddService(frontDeskServiceId, frontDeskActorId, ServiceSource.FrontDesk);
        order.AddDiagnosticService(diagnosticServiceId);
        order.CompleteDiagnostic("Diagnóstico concluído.");

        order.ConsolidateDiagnosticServices();

        var frontDeskItem = order.Services.Single(s => s.ServiceId == frontDeskServiceId);
        frontDeskItem.Source.Should().Be(ServiceSource.FrontDesk);
        frontDeskItem.AddedByActorId.Should().Be(frontDeskActorId);
        frontDeskItem.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ConsolidateDiagnosticServices_UpdatesUpdatedAt()
    {
        var order = CreateOrderInDiagnostic(out _);
        order.AddDiagnosticService(Guid.NewGuid());
        order.CompleteDiagnostic("Diagnóstico concluído.");

        order.ConsolidateDiagnosticServices();

        order.UpdatedAt.Should().NotBeNull();
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
