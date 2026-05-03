namespace GarageFlow.Domain.Parts;

public static class PartConstants
{
    public const int MaxNameLength = 200;
    public const int MaxCodeLength = 50;
    public const int MaxSkuLength = 50;
    public const int MaxUnitOfMeasureLength = 20;

    public static readonly HashSet<string> AllowedUnitsOfMeasure = new(StringComparer.OrdinalIgnoreCase)
    {
        "UN",
        "KG",
        "G",
        "L",
        "ML",
        "M",
        "CM",
        "MM",
        "KIT"
    };
}
