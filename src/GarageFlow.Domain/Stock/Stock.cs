using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Stock;

public sealed class Stock
{
    private List<StockOperation> _operations = [];

    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public StockItemType ItemType { get; private set; }
    public decimal TotalQuantity { get; private set; }
    public decimal AvailableQuantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal MinimumQuantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<StockOperation> Operations => _operations.AsReadOnly();

    private Stock() { }

    public static Stock Create(
        Guid itemId,
        StockItemType itemType,
        decimal initialQuantity,
        decimal minimumQuantity)
    {
        if (itemId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidStockItemId);

        if (initialQuantity < 0)
            throw new DomainException(DomainErrorMessages.InvalidStockInitialQuantity);

        if (minimumQuantity < 0)
            throw new DomainException(DomainErrorMessages.InvalidStockMinimumQuantity);

        return new Stock
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            ItemType = itemType,
            TotalQuantity = initialQuantity,
            ReservedQuantity = 0,
            AvailableQuantity = initialQuantity,
            MinimumQuantity = minimumQuantity,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Entry(decimal quantity, string? reason = null, Guid? referenceId = null)
    {
        var operation = StockOperation.Create(StockOperationType.Entry, quantity, reason, referenceId);

        TotalQuantity += quantity;
        RecalculateAndValidateInvariant();
        Touch();
        _operations.Add(operation);
    }

    public void Reserve(decimal quantity, string? reason = null, Guid? referenceId = null)
    {
        if (quantity <= 0)
            throw new DomainException(DomainErrorMessages.InvalidStockOperationQuantity);

        var currentAvailable = TotalQuantity - ReservedQuantity;
        if (currentAvailable < quantity)
            throw new StockQuantityConflictException(DomainErrorMessages.InsufficientStockAvailability);

        var operation = StockOperation.Create(StockOperationType.Reserve, quantity, reason, referenceId);

        ReservedQuantity += quantity;
        RecalculateAndValidateInvariant();
        Touch();
        _operations.Add(operation);
    }

    public void Release(decimal quantity, string? reason = null, Guid? referenceId = null)
    {
        if (quantity <= 0)
            throw new DomainException(DomainErrorMessages.InvalidStockOperationQuantity);

        if (ReservedQuantity < quantity)
            throw new StockQuantityConflictException(DomainErrorMessages.StockReservedQuantityInsufficient);

        var operation = StockOperation.Create(StockOperationType.Release, quantity, reason, referenceId);

        ReservedQuantity -= quantity;
        RecalculateAndValidateInvariant();
        Touch();
        _operations.Add(operation);
    }

    public void Consume(decimal quantity, string? reason = null, Guid? referenceId = null)
    {
        if (quantity <= 0)
            throw new DomainException(DomainErrorMessages.InvalidStockOperationQuantity);

        if (ReservedQuantity < quantity)
            throw new StockQuantityConflictException(DomainErrorMessages.StockReservedQuantityInsufficient);

        var operation = StockOperation.Create(StockOperationType.Consume, quantity, reason, referenceId);

        ReservedQuantity -= quantity;
        TotalQuantity -= quantity;
        RecalculateAndValidateInvariant();
        Touch();
        _operations.Add(operation);
    }

    public void Adjust(decimal quantityDelta, string reason, Guid? referenceId = null)
    {
        if (quantityDelta == 0)
            throw new DomainException(DomainErrorMessages.InvalidStockAdjustmentQuantity);

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(DomainErrorMessages.StockAdjustmentReasonRequired);

        if (reason.Trim().Length > StockConstants.MaxReasonLength)
            throw new DomainException(DomainErrorMessages.InvalidStockOperationReason);

        var projectedTotal = TotalQuantity + quantityDelta;

        if (projectedTotal < 0 || projectedTotal < ReservedQuantity)
            throw new StockQuantityConflictException(DomainErrorMessages.StockAdjustmentWouldBreakInvariant);

        var operation = StockOperation.Create(StockOperationType.Adjust, quantityDelta, reason, referenceId);

        TotalQuantity = projectedTotal;
        RecalculateAndValidateInvariant();
        Touch();
        _operations.Add(operation);
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateAndValidateInvariant()
    {
        AvailableQuantity = TotalQuantity - ReservedQuantity;

        if (TotalQuantity < 0)
            throw new DomainException(DomainErrorMessages.InvalidStockTotalQuantity);

        if (ReservedQuantity < 0)
            throw new DomainException(DomainErrorMessages.InvalidStockReservedQuantity);

        if (AvailableQuantity < 0)
            throw new DomainException(DomainErrorMessages.InvalidStockAvailableQuantity);
    }
}
