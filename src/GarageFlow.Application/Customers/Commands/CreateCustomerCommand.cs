using GarageFlow.Application.Customers.Enums;

namespace GarageFlow.Application.Customers.Commands;

public sealed record CreateCustomerCommand(
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
    string ZipCode);
