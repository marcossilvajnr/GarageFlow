namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateSupplierDataException(string message) : DomainException(message);
