using GarageFlow.Domain.Customers;

namespace GarageFlow.Api.Customers.DTOs;

public sealed record CreateCustomerRequest(
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
