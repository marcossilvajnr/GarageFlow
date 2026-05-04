namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateDiagnosticServiceException(string message) : DomainException(message);
