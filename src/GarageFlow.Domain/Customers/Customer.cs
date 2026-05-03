using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Domain.Customers;

public sealed class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CustomerDocumentType DocumentType { get; private set; }
    public ValueObjects.Cpf? Cpf { get; private set; }
    public ValueObjects.Cnpj? Cnpj { get; private set; }
    public Email Email { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Customer() { }

    public static Customer Create(
        string name,
        CustomerDocumentType documentType,
        string document,
        string email,
        string phoneNumber,
        Address address)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName))
            throw new DomainException(DomainErrorMessages.InvalidName);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            DocumentType = documentType,
            Email = Email.Create(email),
            PhoneNumber = PhoneNumber.Create(phoneNumber),
            Address = address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        switch (documentType)
        {
            case CustomerDocumentType.Cpf:
                customer.Cpf = ValueObjects.Cpf.Create(document);
                break;
            case CustomerDocumentType.Cnpj:
                customer.Cnpj = ValueObjects.Cnpj.Create(document);
                break;
            default:
                throw new DomainException(DomainErrorMessages.InvalidDocumentType);
        }

        return customer;
    }

    public void Update(string name, string email, string phoneNumber, Address address)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName))
            throw new DomainException(DomainErrorMessages.InvalidName);

        Name = trimmedName;
        Email = Email.Create(email);
        PhoneNumber = PhoneNumber.Create(phoneNumber);
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(DomainErrorMessages.CustomerAlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
