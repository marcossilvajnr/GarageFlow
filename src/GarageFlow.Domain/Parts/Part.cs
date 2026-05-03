using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Parts;

public sealed class Part
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Part() { }

    public static Part Create(
        string name,
        string code,
        string sku,
        string unitOfMeasure,
        decimal unitPrice)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName) || trimmedName.Length > PartConstants.MaxNameLength)
            throw new DomainException(DomainErrorMessages.InvalidPartName);

        var trimmedCode = code?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedCode) || trimmedCode.Length > PartConstants.MaxCodeLength)
            throw new DomainException(DomainErrorMessages.InvalidPartCode);

        var trimmedSku = sku?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedSku) || trimmedSku.Length > PartConstants.MaxSkuLength)
            throw new DomainException(DomainErrorMessages.InvalidPartSku);

        var normalizedUnitOfMeasure = NormalizeUnitOfMeasure(unitOfMeasure);

        if (unitPrice < 0)
            throw new DomainException(DomainErrorMessages.InvalidPartUnitPrice);

        return new Part
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Code = trimmedCode,
            Sku = trimmedSku,
            UnitOfMeasure = normalizedUnitOfMeasure,
            UnitPrice = unitPrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string unitOfMeasure, decimal unitPrice)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName) || trimmedName.Length > PartConstants.MaxNameLength)
            throw new DomainException(DomainErrorMessages.InvalidPartName);

        var normalizedUnitOfMeasure = NormalizeUnitOfMeasure(unitOfMeasure);

        if (unitPrice < 0)
            throw new DomainException(DomainErrorMessages.InvalidPartUnitPrice);

        Name = trimmedName;
        UnitOfMeasure = normalizedUnitOfMeasure;
        UnitPrice = unitPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new DomainException(DomainErrorMessages.InvalidPartUnitPrice);

        UnitPrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(DomainErrorMessages.PartAlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeUnitOfMeasure(string unitOfMeasure)
    {
        var normalized = unitOfMeasure?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(normalized) || normalized.Length > PartConstants.MaxUnitOfMeasureLength)
            throw new DomainException(DomainErrorMessages.InvalidPartUnitOfMeasure);

        if (!PartConstants.AllowedUnitsOfMeasure.Contains(normalized))
            throw new DomainException(DomainErrorMessages.InvalidPartUnitOfMeasure);

        return normalized;
    }
}
