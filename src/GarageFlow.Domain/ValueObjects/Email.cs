using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrorMessages.InvalidEmail);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 320)
            throw new DomainException(DomainErrorMessages.InvalidEmail);

        var atIndex = normalized.IndexOf('@');
        if (atIndex <= 0 || normalized.IndexOf('@', atIndex + 1) >= 0)
            throw new DomainException(DomainErrorMessages.InvalidEmail);

        var domain = normalized[(atIndex + 1)..];
        var lastDot = domain.LastIndexOf('.');
        if (lastDot <= 0 || domain.Length - lastDot - 1 < 2)
            throw new DomainException(DomainErrorMessages.InvalidEmail);

        return new Email(normalized);
    }
}
