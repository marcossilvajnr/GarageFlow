using AppSupplyUnit = GarageFlow.Application.Services.Enums.SupplyUnit;
using DomainSupplyUnit = GarageFlow.Domain.Supplies.SupplyUnit;

namespace GarageFlow.Application.Services.Mappers;

internal static class SupplyUnitMapper
{
    internal static DomainSupplyUnit ToDomain(AppSupplyUnit unit) =>
        unit switch
        {
            AppSupplyUnit.Liter => DomainSupplyUnit.Liter,
            AppSupplyUnit.Milliliter => DomainSupplyUnit.Milliliter,
            AppSupplyUnit.Gram => DomainSupplyUnit.Gram,
            AppSupplyUnit.Kilogram => DomainSupplyUnit.Kilogram,
            AppSupplyUnit.Unit => DomainSupplyUnit.Unit,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };

    internal static AppSupplyUnit ToApplication(DomainSupplyUnit unit) =>
        unit switch
        {
            DomainSupplyUnit.Liter => AppSupplyUnit.Liter,
            DomainSupplyUnit.Milliliter => AppSupplyUnit.Milliliter,
            DomainSupplyUnit.Gram => AppSupplyUnit.Gram,
            DomainSupplyUnit.Kilogram => AppSupplyUnit.Kilogram,
            DomainSupplyUnit.Unit => AppSupplyUnit.Unit,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };
}
