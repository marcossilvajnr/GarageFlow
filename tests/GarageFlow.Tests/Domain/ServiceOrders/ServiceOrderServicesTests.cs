using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Tests.Domain.ServiceOrders;

public sealed class ServiceOrderServicesTests
{
    private static ServiceOrder ValidServiceOrder() =>
        ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

    // AddService tests

    [Fact]
    public void AddService_WithValidArgs_AddsServiceAndHistory()
    {
        var serviceOrder = ValidServiceOrder();
        var serviceId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        serviceOrder.AddService(serviceId, actorId, ServiceSource.FrontDesk);

        serviceOrder.Services.Should().HaveCount(1);
        var item = serviceOrder.Services.Single();
        item.ServiceId.Should().Be(serviceId);
        item.AddedByActorId.Should().Be(actorId);
        item.Source.Should().Be(ServiceSource.FrontDesk);
        item.IsActive.Should().BeTrue();
        item.RemovedAt.Should().BeNull();

        serviceOrder.ServiceHistory.Should().HaveCount(1);
        var history = serviceOrder.ServiceHistory.Single();
        history.ServiceId.Should().Be(serviceId);
        history.Action.Should().Be(ServiceOrderServiceAction.Added);
        history.Source.Should().Be(ServiceSource.FrontDesk);
        history.ActorId.Should().Be(actorId);
        history.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        history.Reason.Should().BeNull();
    }

    [Fact]
    public void AddService_WithEmptyServiceId_ThrowsDomainException()
    {
        var serviceOrder = ValidServiceOrder();

        var act = () => serviceOrder.AddService(Guid.Empty, Guid.NewGuid(), ServiceSource.FrontDesk);

        act.Should().Throw<DomainException>().WithMessage("Id do serviço da OS inválido");
    }

    [Fact]
    public void AddService_WithEmptyActorId_ThrowsDomainException()
    {
        var serviceOrder = ValidServiceOrder();

        var act = () => serviceOrder.AddService(Guid.NewGuid(), Guid.Empty, ServiceSource.FrontDesk);

        act.Should().Throw<DomainException>().WithMessage("Id do ator da operação é inválido");
    }

    [Fact]
    public void AddService_Duplicate_ThrowsDuplicateServiceOrderServiceException()
    {
        var serviceOrder = ValidServiceOrder();
        var serviceId = Guid.NewGuid();
        serviceOrder.AddService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk);

        var act = () => serviceOrder.AddService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk);

        act.Should().Throw<DuplicateServiceOrderServiceException>();
    }

    [Fact]
    public void AddService_UpdatesUpdatedAt()
    {
        var serviceOrder = ValidServiceOrder();
        serviceOrder.UpdatedAt.Should().BeNull();

        serviceOrder.AddService(Guid.NewGuid(), Guid.NewGuid(), ServiceSource.FrontDesk);

        serviceOrder.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // RemoveService tests

    [Fact]
    public void RemoveService_WithValidArgs_RemovesServiceAndPreservesHistory()
    {
        var serviceOrder = ValidServiceOrder();
        var serviceId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        serviceOrder.AddService(serviceId, actorId, ServiceSource.FrontDesk);

        serviceOrder.RemoveService(serviceId, actorId, ServiceSource.FrontDesk, "Cliente desistiu");

        var item = serviceOrder.Services.Single();
        item.IsActive.Should().BeFalse();
        item.RemovedByActorId.Should().Be(actorId);
        item.RemovalReason.Should().Be("Cliente desistiu");
        item.RemovedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        serviceOrder.ServiceHistory.Should().HaveCount(2);
        var removeEntry = serviceOrder.ServiceHistory.Last();
        removeEntry.Action.Should().Be(ServiceOrderServiceAction.Removed);
        removeEntry.Reason.Should().Be("Cliente desistiu");
        removeEntry.ActorId.Should().Be(actorId);
    }

    [Fact]
    public void RemoveService_NotLinked_ThrowsEntityNotFoundException()
    {
        var serviceOrder = ValidServiceOrder();

        var act = () => serviceOrder.RemoveService(Guid.NewGuid(), Guid.NewGuid(), ServiceSource.FrontDesk, "motivo");

        act.Should().Throw<EntityNotFoundException>();
    }

    [Fact]
    public void RemoveService_WithEmptyReason_ThrowsDomainException()
    {
        var serviceOrder = ValidServiceOrder();
        var serviceId = Guid.NewGuid();
        serviceOrder.AddService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk);

        var act = () => serviceOrder.RemoveService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk, "");

        act.Should().Throw<DomainException>().WithMessage("Motivo de remoção do serviço é obrigatório");
    }

    [Fact]
    public void RemoveService_WithWhitespaceReason_ThrowsDomainException()
    {
        var serviceOrder = ValidServiceOrder();
        var serviceId = Guid.NewGuid();
        serviceOrder.AddService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk);

        var act = () => serviceOrder.RemoveService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk, "   ");

        act.Should().Throw<DomainException>().WithMessage("Motivo de remoção do serviço é obrigatório");
    }

    [Fact]
    public void RemoveService_WithEmptyActorId_ThrowsDomainException()
    {
        var serviceOrder = ValidServiceOrder();
        var serviceId = Guid.NewGuid();
        serviceOrder.AddService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk);

        var act = () => serviceOrder.RemoveService(serviceId, Guid.Empty, ServiceSource.FrontDesk, "motivo");

        act.Should().Throw<DomainException>().WithMessage("Id do ator da operação é inválido");
    }

    [Fact]
    public void RemoveService_DoesNotDeleteHistoryEntry()
    {
        var serviceOrder = ValidServiceOrder();
        var serviceId = Guid.NewGuid();
        serviceOrder.AddService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk);

        serviceOrder.RemoveService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk, "motivo");

        // Both add and remove entries must remain
        serviceOrder.ServiceHistory.Should().HaveCount(2);
        serviceOrder.ServiceHistory.Any(h => h.Action == ServiceOrderServiceAction.Added).Should().BeTrue();
        serviceOrder.ServiceHistory.Any(h => h.Action == ServiceOrderServiceAction.Removed).Should().BeTrue();
    }

    [Fact]
    public void RemoveService_AfterRemoval_ServiceCanBeAddedAgain()
    {
        var serviceOrder = ValidServiceOrder();
        var serviceId = Guid.NewGuid();
        serviceOrder.AddService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk);
        serviceOrder.RemoveService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk, "motivo");

        var act = () => serviceOrder.AddService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk);

        act.Should().NotThrow();
        serviceOrder.Services.Count(s => s.IsActive).Should().Be(1);
    }

    [Fact]
    public void AddService_AfterDiagnosticCompleted_ThrowsInvalidServiceOrderStatusTransitionException()
    {
        var serviceOrder = ValidServiceOrder();
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        serviceOrder.AddDiagnosticService(Guid.NewGuid());
        serviceOrder.CompleteDiagnostic("Diagnóstico concluído");

        var act = () => serviceOrder.AddService(Guid.NewGuid(), Guid.NewGuid(), ServiceSource.FrontDesk);

        act.Should()
            .Throw<InvalidServiceOrderStatusTransitionException>()
            .WithMessage("Serviços da OS estão congelados após conclusão do diagnóstico");
    }

    [Fact]
    public void RemoveService_AfterDiagnosticCompleted_ThrowsInvalidServiceOrderStatusTransitionException()
    {
        var serviceOrder = ValidServiceOrder();
        var serviceId = Guid.NewGuid();
        serviceOrder.AddService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk);
        serviceOrder.StartDiagnostic(Guid.NewGuid());
        serviceOrder.AddDiagnosticService(Guid.NewGuid());
        serviceOrder.CompleteDiagnostic("Diagnóstico concluído");

        var act = () => serviceOrder.RemoveService(serviceId, Guid.NewGuid(), ServiceSource.FrontDesk, "Mudança de escopo");

        act.Should()
            .Throw<InvalidServiceOrderStatusTransitionException>()
            .WithMessage("Serviços da OS estão congelados após conclusão do diagnóstico");
    }
}
