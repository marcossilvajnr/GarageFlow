using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.ValueObjects;

public sealed record LicensePlate
{
    public string Value { get; }

    private LicensePlate(string value) => Value = value;

    public static LicensePlate Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrorMessages.InvalidLicensePlate);

        // Remove separators and whitespace, then uppercase
        var normalized = new string(value
            .Where(c => !char.IsWhiteSpace(c) && c != '-')
            .ToArray())
            .ToUpperInvariant();

        // Both formats have 7 characters: LLLNNNN (old) or LLLNLNN (Mercosul)
        if (normalized.Length != 7)
            throw new DomainException(DomainErrorMessages.InvalidLicensePlate);

        // Validate format: must be 3 letters + 4 digits with optional letter in position 4
        if (!IsValidFormat(normalized))
            throw new DomainException(DomainErrorMessages.InvalidLicensePlate);

        return new LicensePlate(normalized);
    }

    private static bool IsValidFormat(string value)
    {
        // Positions 0-2: letters, Position 3-6: mix of digits/letter
        if (!char.IsLetter(value[0]) || !char.IsLetter(value[1]) || !char.IsLetter(value[2]))
            return false;

        // Standard format: LLLNNNN (3 letters + 4 digits)
        var isStandard = char.IsDigit(value[3]) && char.IsDigit(value[4]) &&
                         char.IsDigit(value[5]) && char.IsDigit(value[6]);

        // Mercosul format: LLLNLNN (3 letters + digit + letter + 2 digits)
        var isMercosul = char.IsDigit(value[3]) && char.IsLetter(value[4]) &&
                         char.IsDigit(value[5]) && char.IsDigit(value[6]);

        return isStandard || isMercosul;
    }
}
