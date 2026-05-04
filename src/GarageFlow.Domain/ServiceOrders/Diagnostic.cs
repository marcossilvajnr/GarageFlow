using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.ServiceOrders;

public sealed class Diagnostic
{
    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Guid MechanicId { get; private set; }
    public string? Description { get; private set; }

    // Stored as JSON column in EF Core; public with private setter for persistence access.
    // Domain consumers use the read-only SelectedServices view below.
    public List<Guid> SelectedServiceIds { get; private set; } = [];

    public IReadOnlyList<Guid> SelectedServices => SelectedServiceIds.AsReadOnly();
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DiagnosticStatus Status { get; private set; }

    private Diagnostic() { }

    public static Diagnostic Start(Guid serviceOrderId, Guid mechanicId)
    {
        if (serviceOrderId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderId);

        if (mechanicId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidDiagnosticMechanicId);

        return new Diagnostic
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = serviceOrderId,
            MechanicId = mechanicId,
            StartedAt = DateTime.UtcNow,
            Status = DiagnosticStatus.InProgress,
            SelectedServiceIds = []
        };
    }

    public void AddService(Guid serviceId)
    {
        if (Status == DiagnosticStatus.Completed)
            throw new DiagnosticNotInProgressException(DomainErrorMessages.DiagnosticAlreadyCompleted);

        if (SelectedServiceIds.Contains(serviceId))
            throw new DuplicateDiagnosticServiceException(DomainErrorMessages.DiagnosticServiceAlreadyAdded);

        SelectedServiceIds.Add(serviceId);
    }

    public void RemoveService(Guid serviceId)
    {
        if (Status == DiagnosticStatus.Completed)
            throw new DiagnosticNotInProgressException(DomainErrorMessages.DiagnosticAlreadyCompleted);

        if (!SelectedServiceIds.Contains(serviceId))
            throw new EntityNotFoundException(DomainErrorMessages.DiagnosticServiceNotFound(serviceId));

        if (SelectedServiceIds.Count == DiagnosticConstants.MinimumServiceCount)
            throw new DiagnosticLastServiceException(DomainErrorMessages.DiagnosticMustHaveAtLeastOneService);

        SelectedServiceIds.Remove(serviceId);
    }

    public void Complete(string description)
    {
        if (Status == DiagnosticStatus.Completed)
            throw new DiagnosticNotInProgressException(DomainErrorMessages.DiagnosticAlreadyCompleted);

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(DomainErrorMessages.DiagnosticDescriptionRequired);

        if (SelectedServiceIds.Count == 0)
            throw new DiagnosticNoServicesException(DomainErrorMessages.DiagnosticMustHaveAtLeastOneService);

        Description = description;
        CompletedAt = DateTime.UtcNow;
        Status = DiagnosticStatus.Completed;
    }
}
