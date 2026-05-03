using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Domain.Suppliers;

public sealed class Supplier
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Cnpj Cnpj { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Supplier() { }

    public static Supplier Create(
        string name,
        string cnpj,
        string email,
        string phoneNumber,
        Address address)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName))
            throw new DomainException(DomainErrorMessages.InvalidSupplierName);

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Cnpj = Cnpj.Create(cnpj),
            Email = Email.Create(email),
            PhoneNumber = PhoneNumber.Create(phoneNumber),
            Address = address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return supplier;
    }

    public void Update(string name, string email, string phoneNumber, Address address)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName))
            throw new DomainException(DomainErrorMessages.InvalidSupplierName);

        Name = trimmedName;
        Email = Email.Create(email);
        PhoneNumber = PhoneNumber.Create(phoneNumber);
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(DomainErrorMessages.SupplierAlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
