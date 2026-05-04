using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class RejectQuoteHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task<QuoteDto> HandleAsync(
        RejectQuoteCommand command,
        CancellationToken cancellationToken = default)
    {
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(command.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(command.ServiceOrderId));

        if (serviceOrder.Quote is null)
            throw new QuoteNotFoundException(DomainErrorMessages.QuoteNotFound(command.ServiceOrderId));

        serviceOrder.RejectQuote(command.Reason);

        await serviceOrderRepository.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.ToQuoteDto(serviceOrder.Quote);
    }
}
