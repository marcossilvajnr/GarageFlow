namespace GarageFlow.Application.Customers.Commands;

public sealed record UpdateCustomerCommand(
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
    string ZipCode);
