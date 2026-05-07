using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Purchasing;

public sealed class PurchaseOrder
{
    private List<PurchaseOrderSeparationRef> _separationOrderRefs = [];
    private List<PurchaseItem> _items = [];

    public Guid Id { get; private set; }

    /// <summary>Navigation for EF Core owned collection. Use SeparationOrderIds for domain access.</summary>
    public IReadOnlyList<PurchaseOrderSeparationRef> SeparationOrderRefs => _separationOrderRefs.AsReadOnly();

    public IReadOnlyList<Guid> SeparationOrderIds =>
        _separationOrderRefs.Select(r => r.SeparationOrderId).ToList().AsReadOnly();

    public Guid? SupplierId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public IReadOnlyList<PurchaseItem> Items => _items.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private PurchaseOrder() { }

    public static PurchaseOrder Create(
        IEnumerable<Guid> separationOrderIds,
        IEnumerable<PurchaseItem> items)
    {
        var separationIdsList = separationOrderIds?.ToList() ?? [];
        if (separationIdsList.Count == 0)
            throw new DomainException(DomainErrorMessages.PurchaseOrderMustHaveAtLeastOneSeparationOrder);
        if (separationIdsList.Any(id => id == Guid.Empty))
            throw new DomainException(DomainErrorMessages.InvalidPurchaseOrderSeparationOrderId);

        var itemsList = items?.ToList() ?? [];
        if (itemsList.Count == 0)
            throw new DomainException(DomainErrorMessages.PurchaseOrderMustHaveAtLeastOneItem);

        return new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            Status = PurchaseOrderStatus.Created,
            CreatedAt = DateTime.UtcNow,
            SupplierId = null,
            EmployeeId = null,
            StartedAt = null,
            CompletedAt = null,
            _separationOrderRefs = separationIdsList.Select(PurchaseOrderSeparationRef.From).ToList(),
            _items = itemsList
        };
    }

    public void AssignSupplier(Guid supplierId, Guid employeeId)
    {
        if (supplierId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.PurchaseOrderSupplierRequired);

        if (employeeId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidPurchaseOrderActorEmployeeId);

        if (Status != PurchaseOrderStatus.Created)
            throw new InvalidPurchaseOrderStatusTransitionException(
                DomainErrorMessages.PurchaseOrderCannotChangeSupplierAfterStart);

        SupplierId = supplierId;
        EmployeeId = employeeId;
    }

    public void Start()
    {
        if (Status != PurchaseOrderStatus.Created)
            throw new InvalidPurchaseOrderStatusTransitionException(
                DomainErrorMessages.PurchaseOrderNotCreated);

        if (SupplierId is null)
            throw new DomainException(DomainErrorMessages.PurchaseOrderSupplierNotSet);

        if (EmployeeId is null || EmployeeId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidPurchaseOrderActorEmployeeId);

        Status = PurchaseOrderStatus.Started;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != PurchaseOrderStatus.Started)
            throw new InvalidPurchaseOrderStatusTransitionException(
                DomainErrorMessages.PurchaseOrderNotStarted);

        if (EmployeeId is null || EmployeeId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidPurchaseOrderActorEmployeeId);

        Status = PurchaseOrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

}
