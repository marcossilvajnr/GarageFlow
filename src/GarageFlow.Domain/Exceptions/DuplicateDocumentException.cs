namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateDocumentException(string message) : DomainException(message);
