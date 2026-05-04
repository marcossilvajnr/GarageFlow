using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Services;

public sealed class Service
{
    private List<ServicePartItem> _parts = [];

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public int? EstimatedDurationMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<ServicePartItem> Parts => _parts.AsReadOnly();

    private Service() { }

    public static Service Create(
        string code,
        string name,
        string? description,
        decimal basePrice,
        int? estimatedDurationMinutes)
    {
        var trimmedCode = code?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedCode) || trimmedCode.Length > 50)
            throw new DomainException(DomainErrorMessages.InvalidServiceCode);

        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName) || trimmedName.Length > 200)
            throw new DomainException(DomainErrorMessages.InvalidServiceName);

        var trimmedDescription = description?.Trim();
        if (trimmedDescription is not null && trimmedDescription.Length > 1000)
            throw new DomainException(DomainErrorMessages.InvalidServiceDescription);

        if (basePrice <= 0)
            throw new DomainException(DomainErrorMessages.InvalidServiceBasePrice);

        if (estimatedDurationMinutes.HasValue && estimatedDurationMinutes.Value <= 0)
            throw new DomainException(DomainErrorMessages.InvalidServiceEstimatedDuration);

        return new Service
        {
            Id = Guid.NewGuid(),
            Code = trimmedCode,
            Name = trimmedName,
            Description = string.IsNullOrEmpty(trimmedDescription) ? null : trimmedDescription,
            BasePrice = basePrice,
            EstimatedDurationMinutes = estimatedDurationMinutes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? description,
        decimal basePrice,
        int? estimatedDurationMinutes)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName) || trimmedName.Length > 200)
            throw new DomainException(DomainErrorMessages.InvalidServiceName);

        var trimmedDescription = description?.Trim();
        if (trimmedDescription is not null && trimmedDescription.Length > 1000)
            throw new DomainException(DomainErrorMessages.InvalidServiceDescription);

        if (basePrice <= 0)
            throw new DomainException(DomainErrorMessages.InvalidServiceBasePrice);

        if (estimatedDurationMinutes.HasValue && estimatedDurationMinutes.Value <= 0)
            throw new DomainException(DomainErrorMessages.InvalidServiceEstimatedDuration);

        Name = trimmedName;
        Description = string.IsNullOrEmpty(trimmedDescription) ? null : trimmedDescription;
        BasePrice = basePrice;
        EstimatedDurationMinutes = estimatedDurationMinutes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(DomainErrorMessages.ServiceAlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPart(Guid partId, string partName, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(DomainErrorMessages.InvalidServicePartQuantity);

        if (_parts.Any(p => p.PartId == partId))
            throw new DuplicateServicePartException(DomainErrorMessages.DuplicateServicePart);

        _parts.Add(ServicePartItem.Create(partId, partName, quantity));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePart(Guid partId)
    {
        var item = _parts.FirstOrDefault(p => p.PartId == partId);
        if (item is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServicePartNotFound(Id, partId));

        _parts.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }
}
