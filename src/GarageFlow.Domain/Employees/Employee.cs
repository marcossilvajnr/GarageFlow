using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Domain.Employees;

public sealed class Employee
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Customers.CustomerDocumentType DocumentType { get; private set; }
    public ValueObjects.Cpf? Cpf { get; private set; }
    public ValueObjects.Cnpj? Cnpj { get; private set; }
    public Email Email { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public EmployeeRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Employee() { }

    public static Employee Create(
        string name,
        Customers.CustomerDocumentType documentType,
        string document,
        string email,
        string phoneNumber,
        Address address,
        EmployeeRole role)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName))
            throw new DomainException(DomainErrorMessages.InvalidEmployeeName);

        if (!Enum.IsDefined<EmployeeRole>(role))
            throw new DomainException(DomainErrorMessages.InvalidEmployeeRole);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            DocumentType = documentType,
            Email = Email.Create(email),
            PhoneNumber = PhoneNumber.Create(phoneNumber),
            Address = address,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        switch (documentType)
        {
            case Customers.CustomerDocumentType.Cpf:
                employee.Cpf = ValueObjects.Cpf.Create(document);
                break;
            case Customers.CustomerDocumentType.Cnpj:
                employee.Cnpj = ValueObjects.Cnpj.Create(document);
                break;
            default:
                throw new DomainException(DomainErrorMessages.InvalidEmployeeDocumentType);
        }

        return employee;
    }

    public void Update(string name, string email, string phoneNumber, Address address, EmployeeRole role)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName))
            throw new DomainException(DomainErrorMessages.InvalidEmployeeName);

        if (!Enum.IsDefined<EmployeeRole>(role))
            throw new DomainException(DomainErrorMessages.InvalidEmployeeRole);

        Name = trimmedName;
        Email = Email.Create(email);
        PhoneNumber = PhoneNumber.Create(phoneNumber);
        Address = address;
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(DomainErrorMessages.EmployeeAlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}