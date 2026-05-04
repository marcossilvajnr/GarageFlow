using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Executions;

namespace GarageFlow.Tests.Domain.Executions;

public sealed class ExecutionOrderTests
{
    // --- Create ---

    [Fact]
    public void Create_WithValidData_ReturnsPendingExecutionOrder()
    {
        var serviceOrderId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var order = ExecutionOrder.Create(serviceOrderId, serviceId);

        order.Id.Should().NotBeEmpty();
        order.ServiceOrderId.Should().Be(serviceOrderId);
        order.ServiceId.Should().Be(serviceId);
        order.Status.Should().Be(ExecutionOrderStatus.Pending);
        order.MechanicId.Should().BeNull();
        order.StartedAt.Should().BeNull();
        order.CompletedAt.Should().BeNull();
        order.ActualTimeMinutes.Should().BeNull();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyServiceOrderId_ThrowsDomainException()
    {
        var act = () => ExecutionOrder.Create(Guid.Empty, Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("OS é obrigatória");
    }

    [Fact]
    public void Create_WithEmptyServiceId_ThrowsDomainException()
    {
        var act = () => ExecutionOrder.Create(Guid.NewGuid(), Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("Serviço é obrigatório");
    }

    [Fact]
    public void Create_ServiceOrderIdIsImmutable()
    {
        var serviceOrderId = Guid.NewGuid();
        var order = ExecutionOrder.Create(serviceOrderId, Guid.NewGuid());

        order.ServiceOrderId.Should().Be(serviceOrderId);
    }

    [Fact]
    public void Create_ServiceIdIsImmutable()
    {
        var serviceId = Guid.NewGuid();
        var order = ExecutionOrder.Create(Guid.NewGuid(), serviceId);

        order.ServiceId.Should().Be(serviceId);
    }

    // --- MarkReadyToStart ---

    [Fact]
    public void MarkReadyToStart_WhenPending_ChangesStatusToReady()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        order.MarkReadyToStart();

        order.Status.Should().Be(ExecutionOrderStatus.Ready);
    }

    [Fact]
    public void MarkReadyToStart_WhenAlreadyReady_IsIdempotentNoError()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.MarkReadyToStart();

        var act = () => order.MarkReadyToStart();

        act.Should().NotThrow();
        order.Status.Should().Be(ExecutionOrderStatus.Ready);
    }

    [Fact]
    public void MarkReadyToStart_WhenInExecution_IsIdempotentNoError()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.MarkReadyToStart();
        order.StartExecution(Guid.NewGuid());

        var act = () => order.MarkReadyToStart();

        act.Should().NotThrow();
        order.Status.Should().Be(ExecutionOrderStatus.InExecution);
    }

    [Fact]
    public void MarkReadyToStart_WhenCompleted_IsIdempotentNoError()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.MarkReadyToStart();
        order.StartExecution(Guid.NewGuid());
        order.CompleteExecution();

        var act = () => order.MarkReadyToStart();

        act.Should().NotThrow();
        order.Status.Should().Be(ExecutionOrderStatus.Completed);
    }

    // --- StartExecution ---

    [Fact]
    public void StartExecution_WhenReady_SetsInExecutionAndMechanic()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.MarkReadyToStart();
        var mechanicId = Guid.NewGuid();

        order.StartExecution(mechanicId);

        order.Status.Should().Be(ExecutionOrderStatus.InExecution);
        order.MechanicId.Should().Be(mechanicId);
        order.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void StartExecution_WhenPending_ThrowsInvalidExecutionOrderStatusTransitionException()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => order.StartExecution(Guid.NewGuid());

        act.Should().Throw<InvalidExecutionOrderStatusTransitionException>()
            .WithMessage("Ordem de Execução não está Pronta para Início");
    }

    [Fact]
    public void StartExecution_WhenCompleted_ThrowsInvalidExecutionOrderStatusTransitionException()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.MarkReadyToStart();
        order.StartExecution(Guid.NewGuid());
        order.CompleteExecution();

        var act = () => order.StartExecution(Guid.NewGuid());

        act.Should().Throw<InvalidExecutionOrderStatusTransitionException>();
    }

    [Fact]
    public void StartExecution_WithEmptyMechanicId_ThrowsDomainException()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.MarkReadyToStart();

        var act = () => order.StartExecution(Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("Mecânico é obrigatório");
    }

    // --- CompleteExecution ---

    [Fact]
    public void CompleteExecution_WhenInExecution_SetsCompletedAndCalculatesTime()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.MarkReadyToStart();
        order.StartExecution(Guid.NewGuid());

        order.CompleteExecution();

        order.Status.Should().Be(ExecutionOrderStatus.Completed);
        order.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.ActualTimeMinutes.Should().NotBeNull();
        order.ActualTimeMinutes.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public void CompleteExecution_WhenPending_ThrowsInvalidExecutionOrderStatusTransitionException()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => order.CompleteExecution();

        act.Should().Throw<InvalidExecutionOrderStatusTransitionException>()
            .WithMessage("Ordem de Execução não está Em Execução");
    }

    [Fact]
    public void CompleteExecution_WhenReady_ThrowsInvalidExecutionOrderStatusTransitionException()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.MarkReadyToStart();

        var act = () => order.CompleteExecution();

        act.Should().Throw<InvalidExecutionOrderStatusTransitionException>();
    }

    [Fact]
    public void CompleteExecution_ActualTimeMinutes_IsDecimalCalculation()
    {
        var order = ExecutionOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.MarkReadyToStart();
        order.StartExecution(Guid.NewGuid());

        order.CompleteExecution();

        // ActualTimeMinutes should be a non-negative decimal
        order.ActualTimeMinutes.Should().NotBeNull();
        order.ActualTimeMinutes!.Value.Should().BeGreaterThanOrEqualTo(0m);
    }
}
