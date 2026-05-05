using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Stock.Handlers;

internal static class StockItemExistenceValidator
{
    public static async Task EnsureExistsAsync(
        Guid itemId,
        StockItemType itemType,
        IPartRepository partRepository,
        ISupplyRepository supplyRepository,
        CancellationToken cancellationToken)
    {
        if (itemType == StockItemType.Part)
        {
            var part = await partRepository.GetByIdAsync(itemId, cancellationToken);
            if (part is null)
                throw new EntityNotFoundException(DomainErrorMessages.PartNotFound(itemId));

            return;
        }

        var supply = await supplyRepository.GetByIdAsync(itemId, cancellationToken);
        if (supply is null)
            throw new EntityNotFoundException(DomainErrorMessages.SupplyNotFound(itemId));
    }
}
