using GarageFlow.Application.Employees.Enums;

namespace GarageFlow.Application.Employees.Commands;

public sealed record UpdateEmployeeCommand(
    Guid Id,
    string Name,
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
