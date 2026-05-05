using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.ServiceOrders;

public sealed class ServiceOrder
{
    private List<ServiceOrderServiceItem> _services = [];
    private List<ServiceOrderServiceHistory> _serviceHistory = [];

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid VehicleId { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public Diagnostic? Diagnostic { get; private set; }
    public Quote? Quote { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<ServiceOrderServiceItem> Services => _services.AsReadOnly();
    public IReadOnlyList<ServiceOrderServiceHistory> ServiceHistory => _serviceHistory.AsReadOnly();

    private ServiceOrder() { }

    public static ServiceOrder Create(Guid customerId, Guid vehicleId)
    {
        if (customerId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderCustomerId);

        if (vehicleId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderVehicleId);

        return new ServiceOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicleId,
            Status = ServiceOrderStatus.Received,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddService(Guid serviceId, Guid actorId, ServiceSource source)
    {
        if (serviceId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderServiceId);

        if (actorId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderActorId);

        if (_services.Any(s => s.ServiceId == serviceId && s.IsActive))
            throw new DuplicateServiceOrderServiceException(DomainErrorMessages.ServiceOrderServiceAlreadyActive);

        var occurredAt = DateTime.UtcNow;

        var item = ServiceOrderServiceItem.Create(serviceId, actorId, source);
        _services.Add(item);

        var historyEntry = ServiceOrderServiceHistory.Create(
            serviceId, ServiceOrderServiceAction.Added, source, actorId, occurredAt);
        _serviceHistory.Add(historyEntry);

        UpdatedAt = occurredAt;
    }

    public void RemoveService(Guid serviceId, Guid actorId, ServiceSource source, string reason)
    {
        if (serviceId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderServiceId);

        if (actorId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderActorId);

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(DomainErrorMessages.ServiceOrderServiceRemovalReasonRequired);

        var item = _services.FirstOrDefault(s => s.ServiceId == serviceId && s.IsActive);
        if (item is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderServiceNotFound(serviceId));

        var occurredAt = DateTime.UtcNow;

        item.Remove(actorId, reason, occurredAt);

        var historyEntry = ServiceOrderServiceHistory.Create(
            serviceId, ServiceOrderServiceAction.Removed, source, actorId, occurredAt, reason);
        _serviceHistory.Add(historyEntry);

        UpdatedAt = occurredAt;
    }

    public void StartDiagnostic(Guid mechanicId)
    {
        if (mechanicId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidDiagnosticMechanicId);

        if (Status != ServiceOrderStatus.Received)
            throw new DiagnosticAlreadyStartedException(DomainErrorMessages.DiagnosticAlreadyStarted);

        Diagnostic = Diagnostic.Start(Id, mechanicId);
        Status = ServiceOrderStatus.InDiagnostic;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDiagnosticService(Guid serviceId)
    {
        if (Diagnostic is null)
            throw new DiagnosticNotInProgressException(DomainErrorMessages.DiagnosticNotStarted);

        Diagnostic.AddService(serviceId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveDiagnosticService(Guid serviceId)
    {
        if (Diagnostic is null)
            throw new DiagnosticNotInProgressException(DomainErrorMessages.DiagnosticNotStarted);

        Diagnostic.RemoveService(serviceId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteDiagnostic(string description)
    {
        if (Diagnostic is null)
            throw new DiagnosticNotInProgressException(DomainErrorMessages.DiagnosticNotStarted);

        Diagnostic.Complete(description);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConsolidateDiagnosticServices()
    {
        if (Diagnostic is null)
            throw new DiagnosticNotCompletedException(DomainErrorMessages.DiagnosticNotStarted);

        if (Diagnostic.Status != DiagnosticStatus.Completed)
            throw new DiagnosticNotCompletedException(DomainErrorMessages.DiagnosticNotCompleted);

        if (Diagnostic.SelectedServices.Count == 0)
            throw new DiagnosticNoServicesException(DomainErrorMessages.DiagnosticConsolidationNoServices);

        var mechanicId = Diagnostic.MechanicId;
        var occurredAt = DateTime.UtcNow;

        foreach (var serviceId in Diagnostic.SelectedServices)
        {
            if (_services.Any(s => s.ServiceId == serviceId && s.IsActive))
                continue;

            var item = ServiceOrderServiceItem.Create(serviceId, mechanicId, ServiceSource.Diagnostic);
            _services.Add(item);

            var historyEntry = ServiceOrderServiceHistory.Create(
                serviceId, ServiceOrderServiceAction.Added, ServiceSource.Diagnostic, mechanicId, occurredAt);
            _serviceHistory.Add(historyEntry);
        }

        UpdatedAt = occurredAt;
    }

    public void GenerateQuote(IEnumerable<QuoteItem> items)
    {
        if (Quote is not null)
            throw new QuoteAlreadyExistsException(DomainErrorMessages.QuoteAlreadyExists);

        if (!_services.Any(s => s.IsActive))
            throw new NoConsolidatedServicesException(DomainErrorMessages.QuoteNoConsolidatedServices);

        Quote = Quote.Generate(Id, items);
        Status = ServiceOrderStatus.WaitingApproval;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcceptQuote()
    {
        if (Quote is null)
            throw new QuoteNotFoundException(DomainErrorMessages.QuoteNotFound(Id));

        if (Status != ServiceOrderStatus.WaitingApproval)
            throw new QuoteAlreadyDecidedException(DomainErrorMessages.ServiceOrderNotWaitingForQuoteApproval);

        Quote.Accept();
        Status = ServiceOrderStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RejectQuote(string reason)
    {
        if (Quote is null)
            throw new QuoteNotFoundException(DomainErrorMessages.QuoteNotFound(Id));

        if (Status != ServiceOrderStatus.WaitingApproval)
            throw new QuoteAlreadyDecidedException(DomainErrorMessages.ServiceOrderNotWaitingForQuoteApproval);

        Quote.Reject(reason);
        Status = ServiceOrderStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Finish()
    {
        if (Status != ServiceOrderStatus.InExecution)
            throw new InvalidServiceOrderStatusTransitionException(DomainErrorMessages.ServiceOrderCannotFinish);

        Status = ServiceOrderStatus.Finished;
        UpdatedAt = DateTime.UtcNow;
    }
}
