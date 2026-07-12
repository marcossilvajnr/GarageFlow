using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Application.ServiceOrders.Enums;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class HandleExternalQuoteDecisionHandler(
    AcceptQuoteHandler acceptQuoteHandler,
    RejectQuoteHandler rejectQuoteHandler,
    ILogger<HandleExternalQuoteDecisionHandler> logger)
{
    public async Task<QuoteDto> HandleAsync(
        HandleExternalQuoteDecisionCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "external_quote_decision_received serviceOrderId={ServiceOrderId} decision={Decision} source={Source} externalNotificationId={ExternalNotificationId}",
            command.ServiceOrderId,
            command.Decision,
            command.Source,
            command.ExternalNotificationId);

        if (command.ServiceOrderId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.ExternalServiceOrderIdRequired);

        if (string.IsNullOrWhiteSpace(command.Source))
            throw new DomainException(DomainErrorMessages.ExternalSourceRequired);

        var decision = ParseDecision(command.Decision);

        if (decision == ExternalQuoteDecision.Rejected && string.IsNullOrWhiteSpace(command.Reason))
            throw new DomainException(DomainErrorMessages.QuoteRejectionReasonRequired);

        QuoteDto dto;
        try
        {
            dto = decision == ExternalQuoteDecision.Approved
                ? await acceptQuoteHandler.HandleAsync(new AcceptQuoteCommand(command.ServiceOrderId), cancellationToken)
                : await rejectQuoteHandler.HandleAsync(new RejectQuoteCommand(command.ServiceOrderId, command.Reason!), cancellationToken);
        }
        catch (QuoteAlreadyDecidedException ex)
        {
            throw new ExternalQuoteDecisionConflictException(ex.Message);
        }

        logger.LogInformation(
            "external_quote_decision_processed serviceOrderId={ServiceOrderId} decision={Decision} quoteId={QuoteId} quoteStatus={QuoteStatus} source={Source} externalNotificationId={ExternalNotificationId}",
            command.ServiceOrderId,
            decision,
            dto.Id,
            dto.Status,
            command.Source,
            command.ExternalNotificationId);

        return dto;
    }

    private static ExternalQuoteDecision ParseDecision(string? decision) =>
        decision switch
        {
            nameof(ExternalQuoteDecision.Approved) => ExternalQuoteDecision.Approved,
            nameof(ExternalQuoteDecision.Rejected) => ExternalQuoteDecision.Rejected,
            _ => throw new DomainException(DomainErrorMessages.ExternalQuoteDecisionInvalid)
        };
}
