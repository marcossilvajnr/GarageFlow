using GarageFlow.Application.Employees.Enums;

namespace GarageFlow.Api.Employees.DTOs;

public sealed record UpdateEmployeeRequest(
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
