namespace GarageFlow.Domain.Supplies;

public static class SupplyConstants
{
    public const int MaxNameLength = 200;
    public const int MaxCodeLength = 50;
    public const int MaxUnitOfMeasureLength = 20;
    public const decimal MinBaseCost = 0m;

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
