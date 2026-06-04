using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

using ApplicationCustomerDocumentType = GarageFlow.Application.Customers.Enums.CustomerDocumentType;
using DomainCustomerDocumentType = GarageFlow.Domain.Customers.CustomerDocumentType;

namespace GarageFlow.Application.Employees.Mappers;

internal static class EmployeeDocumentTypeMapper
{
    internal static DomainCustomerDocumentType ToDomain(ApplicationCustomerDocumentType documentType) =>
        documentType switch
        {
            ApplicationCustomerDocumentType.Cpf => DomainCustomerDocumentType.Cpf,
            ApplicationCustomerDocumentType.Cnpj => DomainCustomerDocumentType.Cnpj,
            _ => throw new DomainException(DomainErrorMessages.InvalidEmployeeDocumentType)
        };

    internal static ApplicationCustomerDocumentType ToApplication(DomainCustomerDocumentType documentType) =>
        documentType switch
        {
            DomainCustomerDocumentType.Cpf => ApplicationCustomerDocumentType.Cpf,
            DomainCustomerDocumentType.Cnpj => ApplicationCustomerDocumentType.Cnpj,
            _ => throw new DomainException(DomainErrorMessages.InvalidEmployeeDocumentType)
        };
}
