using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class GenerateQuoteHandler(
    IServiceOrderRepository serviceOrderRepository,
    IServiceRepository serviceRepository,
    IPartRepository partRepository,
    ISupplyRepository supplyRepository)
{
    public async Task<QuoteDto> HandleAsync(
        GenerateQuoteCommand command,
        CancellationToken cancellationToken = default)
    {
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(command.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(command.ServiceOrderId));

        var activeServices = serviceOrder.Services.Where(s => s.IsActive).ToList();
        if (activeServices.Count == 0)
            throw new NoConsolidatedServicesException(DomainErrorMessages.QuoteNoConsolidatedServices);

        var quoteItems = new List<QuoteItem>();

        foreach (var serviceItem in activeServices)
        {
            var service = await serviceRepository.GetByIdAsync(serviceItem.ServiceId, cancellationToken);
            if (service is null || !service.IsActive)
                throw new ServiceNotAvailableForQuoteException(
                    DomainErrorMessages.ServiceNotAvailableForQuote(serviceItem.ServiceId));

            var partsTotal = 0m;
            foreach (var part in service.Parts)
            {
                var partEntity = await partRepository.GetByIdAsync(part.PartId, cancellationToken);
                if (partEntity is not null && partEntity.IsActive)
                    partsTotal += partEntity.UnitPrice * part.Quantity;
            }

            var suppliesTotal = 0m;
            foreach (var supply in service.Supplies)
            {
                var supplyEntity = await supplyRepository.GetByIdAsync(supply.SupplyId, cancellationToken);
                if (supplyEntity is not null && supplyEntity.IsActive)
                    suppliesTotal += supplyEntity.BaseCost * supply.Quantity;
            }

            quoteItems.Add(QuoteItem.Create(
                service.Id,
                service.Name,
                service.BasePrice,
                partsTotal,
                suppliesTotal));
        }

        serviceOrder.GenerateQuote(quoteItems);

        await serviceOrderRepository.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.ToQuoteDto(serviceOrder.Quote!);
    }
}
