using GarageFlow.Application.Customers.Enums;
using GarageFlow.Application.Employees.Enums;

namespace GarageFlow.Application.Employees.Commands;

public sealed record CreateEmployeeCommand(
    string Name,
    CustomerDocumentType DocumentType,
    string Document,
    string Email,
    string PhoneNumber,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    EmployeeRole Role);
