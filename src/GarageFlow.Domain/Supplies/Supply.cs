using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Supplies;

public sealed class Supply
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal BaseCost { get; private set; }
    public Guid? PreferredSupplierId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Supply() { }

    public static Supply Create(
        string name,
        string code,
        string unitOfMeasure,
        decimal baseCost,
        Guid? preferredSupplierId = null)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName) || trimmedName.Length > SupplyConstants.MaxNameLength)
            throw new DomainException(DomainErrorMessages.InvalidSupplyName);

        var trimmedCode = code?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedCode) || trimmedCode.Length > SupplyConstants.MaxCodeLength)
            throw new DomainException(DomainErrorMessages.InvalidSupplyCode);

        var normalizedUnitOfMeasure = NormalizeUnitOfMeasure(unitOfMeasure);

        if (baseCost < SupplyConstants.MinBaseCost)
            throw new DomainException(DomainErrorMessages.InvalidSupplyBaseCost);

        return new Supply
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Code = trimmedCode,
            UnitOfMeasure = normalizedUnitOfMeasure,
            BaseCost = baseCost,
            PreferredSupplierId = preferredSupplierId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string unitOfMeasure, decimal baseCost, Guid? preferredSupplierId)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName) || trimmedName.Length > SupplyConstants.MaxNameLength)
            throw new DomainException(DomainErrorMessages.InvalidSupplyName);

        var normalizedUnitOfMeasure = NormalizeUnitOfMeasure(unitOfMeasure);

        if (baseCost < SupplyConstants.MinBaseCost)
            throw new DomainException(DomainErrorMessages.InvalidSupplyBaseCost);

        Name = trimmedName;
        UnitOfMeasure = normalizedUnitOfMeasure;
        BaseCost = baseCost;
        PreferredSupplierId = preferredSupplierId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(DomainErrorMessages.SupplyAlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeUnitOfMeasure(string unitOfMeasure)
    {
        var normalized = unitOfMeasure?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(normalized) || normalized.Length > SupplyConstants.MaxUnitOfMeasureLength)
            throw new DomainException(DomainErrorMessages.InvalidSupplyUnitOfMeasure);

        if (!SupplyConstants.AllowedUnitsOfMeasure.Contains(normalized))
            throw new DomainException(DomainErrorMessages.InvalidSupplyUnitOfMeasure);

        return normalized;
    }
}
