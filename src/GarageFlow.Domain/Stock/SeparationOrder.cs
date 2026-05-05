using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Stock;

public sealed class SeparationOrder
{
    private List<SeparationPartItem> _parts = [];
    private List<SeparationSupplyItem> _supplies = [];

    public Guid Id { get; private set; }
    public Guid ExecutionOrderId { get; private set; }
    public SeparationOrderStatus Status { get; private set; }
    public Guid? StockistId { get; private set; }
    public DateTime? ConfirmedByStockistAt { get; private set; }
    public DateTime? ConfirmedByMechanicAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<SeparationPartItem> Parts => _parts.AsReadOnly();
    public IReadOnlyList<SeparationSupplyItem> Supplies => _supplies.AsReadOnly();

    private SeparationOrder() { }

    public static SeparationOrder Create(
        Guid executionOrderId,
        IEnumerable<SeparationPartItem> parts,
        IEnumerable<SeparationSupplyItem> supplies)
    {
        if (executionOrderId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidSeparationOrderExecutionOrderId);

        var partsList = parts?.ToList() ?? [];
        var suppliesList = supplies?.ToList() ?? [];

        if (partsList.Count == 0 && suppliesList.Count == 0)
            throw new DomainException(DomainErrorMessages.SeparationOrderMustHaveAtLeastOneItem);

        var duplicatePart = partsList.GroupBy(p => p.PartId).FirstOrDefault(g => g.Count() > 1);
        if (duplicatePart is not null)
            throw new DomainException(DomainErrorMessages.DuplicateSeparationPartItem);

        var duplicateSupply = suppliesList.GroupBy(s => s.SupplyId).FirstOrDefault(g => g.Count() > 1);
        if (duplicateSupply is not null)
            throw new DomainException(DomainErrorMessages.DuplicateSeparationSupplyItem);

        return new SeparationOrder
        {
            Id = Guid.NewGuid(),
            ExecutionOrderId = executionOrderId,
            Status = SeparationOrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            _parts = partsList,
            _supplies = suppliesList
        };
    }

    public void Reserve()
    {
        if (Status != SeparationOrderStatus.Pending)
            throw new InvalidSeparationOrderStatusTransitionException(
                DomainErrorMessages.SeparationOrderNotPending);

        foreach (var part in _parts) part.MarkReserved();
        foreach (var supply in _supplies) supply.MarkReserved();

        Status = SeparationOrderStatus.WaitingPickup;
    }

    public void WaitForPurchase()
    {
        if (Status != SeparationOrderStatus.Pending)
            throw new InvalidSeparationOrderStatusTransitionException(
                DomainErrorMessages.SeparationOrderNotPending);

        Status = SeparationOrderStatus.WaitingPurchase;
    }

    public void ResumeAfterPurchase()
    {
        if (Status != SeparationOrderStatus.WaitingPurchase)
            throw new InvalidSeparationOrderStatusTransitionException(
                DomainErrorMessages.SeparationOrderNotWaitingPurchase);

        foreach (var part in _parts) part.MarkReserved();
        foreach (var supply in _supplies) supply.MarkReserved();

        Status = SeparationOrderStatus.WaitingPickup;
    }

    public void ConfirmStockistWithdrawal(Guid stockistId)
    {
        if (Status != SeparationOrderStatus.WaitingPickup)
            throw new InvalidSeparationOrderStatusTransitionException(
                DomainErrorMessages.SeparationOrderNotWaitingPickup);

        if (stockistId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidSeparationStockistId);

        var allReserved = _parts.All(p => p.IsReserved) && _supplies.All(s => s.IsReserved);
        if (!allReserved)
            throw new SeparationOrderCustodyPreconditionException(
                DomainErrorMessages.SeparationOrderItemsNotReserved);

        StockistId = stockistId;
        ConfirmedByStockistAt = DateTime.UtcNow;
        Status = SeparationOrderStatus.Separated;
    }

    public void ConfirmMechanicReceipt()
    {
        if (Status != SeparationOrderStatus.Separated)
            throw new InvalidSeparationOrderStatusTransitionException(
                DomainErrorMessages.SeparationOrderNotSeparated);

        if (ConfirmedByStockistAt is null)
            throw new SeparationOrderCustodyPreconditionException(
                DomainErrorMessages.SeparationOrderWaitingStockistConfirmation);

        ConfirmedByMechanicAt = DateTime.UtcNow;
        Status = SeparationOrderStatus.Completed;
    }

    public void EnsureEligibleForTotalReturnBeforeMechanicReceipt()
    {
        if (Status != SeparationOrderStatus.Separated || ConfirmedByStockistAt is null || ConfirmedByMechanicAt is not null)
            throw new SeparationOrderCustodyPreconditionException(
                DomainErrorMessages.SeparationOrderNotEligibleForTotalReturn);
    }

    public void ReturnTotalBeforeMechanicReceipt()
    {
        EnsureEligibleForTotalReturnBeforeMechanicReceipt();

        foreach (var part in _parts) part.UnmarkReserved();
        foreach (var supply in _supplies) supply.UnmarkReserved();

        StockistId = null;
        ConfirmedByStockistAt = null;
        Status = SeparationOrderStatus.Pending;
    }
}
