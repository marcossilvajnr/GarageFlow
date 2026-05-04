using FluentAssertions;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Tests.Domain.ServiceOrders;

public sealed class QuoteTests
{
    private static QuoteItem ValidItem(Guid? serviceId = null) =>
        QuoteItem.Create(
            serviceId ?? Guid.NewGuid(),
            "Troca de Óleo",
            laborPrice: 100m,
            partsTotal: 50m,
            suppliesTotal: 20m);

    // ── QuoteItem.Create ────────────────────────────────────────────────────

    [Fact]
    public void QuoteItemCreate_WithValidData_SetsSubtotalCorrectly()
    {
        var item = QuoteItem.Create(Guid.NewGuid(), "Alinhamento", 150m, 40m, 10m);

        item.LaborPrice.Should().Be(150m);
        item.PartsTotal.Should().Be(40m);
        item.SuppliesTotal.Should().Be(10m);
        item.Subtotal.Should().Be(200m);
    }

    [Fact]
    public void QuoteItemCreate_WithZeroValues_SetsSubtotalZero()
    {
        var item = QuoteItem.Create(Guid.NewGuid(), "Inspeção", 0m, 0m, 0m);

        item.Subtotal.Should().Be(0m);
    }

    [Fact]
    public void QuoteItemCreate_WithEmptyServiceId_ThrowsDomainException()
    {
        var act = () => QuoteItem.Create(Guid.Empty, "Serviço", 100m, 0m, 0m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void QuoteItemCreate_WithNegativeLaborPrice_ThrowsDomainException()
    {
        var act = () => QuoteItem.Create(Guid.NewGuid(), "Serviço", -1m, 0m, 0m);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.QuoteInvalidLaborPrice);
    }

    [Fact]
    public void QuoteItemCreate_WithNegativePartsTotal_ThrowsDomainException()
    {
        var act = () => QuoteItem.Create(Guid.NewGuid(), "Serviço", 0m, -1m, 0m);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.QuoteInvalidPartsTotal);
    }

    [Fact]
    public void QuoteItemCreate_WithNegativeSuppliesTotal_ThrowsDomainException()
    {
        var act = () => QuoteItem.Create(Guid.NewGuid(), "Serviço", 0m, 0m, -1m);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.QuoteInvalidSuppliesTotal);
    }

    // ── Quote.Generate ──────────────────────────────────────────────────────

    [Fact]
    public void QuoteGenerate_WithValidItems_CreatesPendingQuote()
    {
        var serviceOrderId = Guid.NewGuid();
        var items = new[] { ValidItem() };

        var quote = Quote.Generate(serviceOrderId, items);

        quote.Id.Should().NotBeEmpty();
        quote.ServiceOrderId.Should().Be(serviceOrderId);
        quote.Status.Should().Be(QuoteStatus.WaitingCustomerApproval);
        quote.Items.Should().HaveCount(1);
        quote.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        quote.AcceptedAt.Should().BeNull();
        quote.RejectedAt.Should().BeNull();
    }

    [Fact]
    public void QuoteGenerate_TotalAmountEqualsSumOfSubtotals()
    {
        var items = new[]
        {
            QuoteItem.Create(Guid.NewGuid(), "Serviço A", 100m, 50m, 20m), // subtotal = 170
            QuoteItem.Create(Guid.NewGuid(), "Serviço B", 200m, 30m, 10m)  // subtotal = 240
        };

        var quote = Quote.Generate(Guid.NewGuid(), items);

        quote.TotalAmount.Should().Be(410m);
    }

    [Fact]
    public void QuoteGenerate_WithEmptyItems_ThrowsDomainException()
    {
        var act = () => Quote.Generate(Guid.NewGuid(), []);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.QuoteRequiresAtLeastOneItem);
    }

    [Fact]
    public void QuoteGenerate_WithEmptyServiceOrderId_ThrowsDomainException()
    {
        var act = () => Quote.Generate(Guid.Empty, [ValidItem()]);

        act.Should().Throw<DomainException>();
    }

    // ── Quote.Accept ────────────────────────────────────────────────────────

    [Fact]
    public void QuoteAccept_WhenPending_SetsAcceptedStatus()
    {
        var quote = Quote.Generate(Guid.NewGuid(), [ValidItem()]);

        quote.Accept();

        quote.Status.Should().Be(QuoteStatus.CustomerApproved);
        quote.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void QuoteAccept_WhenAlreadyAccepted_ThrowsQuoteAlreadyDecidedException()
    {
        var quote = Quote.Generate(Guid.NewGuid(), [ValidItem()]);
        quote.Accept();

        var act = () => quote.Accept();

        act.Should().Throw<QuoteAlreadyDecidedException>()
            .WithMessage(DomainErrorMessages.QuoteAlreadyDecided);
    }

    [Fact]
    public void QuoteAccept_WhenRejected_ThrowsQuoteAlreadyDecidedException()
    {
        var quote = Quote.Generate(Guid.NewGuid(), [ValidItem()]);
        quote.Reject("Valor acima do esperado");

        var act = () => quote.Accept();

        act.Should().Throw<QuoteAlreadyDecidedException>()
            .WithMessage(DomainErrorMessages.QuoteAlreadyDecided);
    }

    // ── Quote.Reject ────────────────────────────────────────────────────────

    [Fact]
    public void QuoteReject_WhenPendingWithReason_SetsRejectedStatus()
    {
        var quote = Quote.Generate(Guid.NewGuid(), [ValidItem()]);
        const string reason = "Preço elevado";

        quote.Reject(reason);

        quote.Status.Should().Be(QuoteStatus.CustomerRejected);
        quote.RejectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        quote.RejectionReason.Should().Be(reason);
    }

    [Fact]
    public void QuoteReject_WithEmptyReason_ThrowsDomainException()
    {
        var quote = Quote.Generate(Guid.NewGuid(), [ValidItem()]);

        var act = () => quote.Reject(string.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.QuoteRejectionReasonRequired);
    }

    [Fact]
    public void QuoteReject_WithWhitespaceReason_ThrowsDomainException()
    {
        var quote = Quote.Generate(Guid.NewGuid(), [ValidItem()]);

        var act = () => quote.Reject("   ");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainErrorMessages.QuoteRejectionReasonRequired);
    }

    [Fact]
    public void QuoteReject_WhenAlreadyRejected_ThrowsQuoteAlreadyDecidedException()
    {
        var quote = Quote.Generate(Guid.NewGuid(), [ValidItem()]);
        quote.Reject("Primeira rejeição");

        var act = () => quote.Reject("Segunda rejeição");

        act.Should().Throw<QuoteAlreadyDecidedException>()
            .WithMessage(DomainErrorMessages.QuoteAlreadyDecided);
    }

    [Fact]
    public void QuoteReject_WhenAccepted_ThrowsQuoteAlreadyDecidedException()
    {
        var quote = Quote.Generate(Guid.NewGuid(), [ValidItem()]);
        quote.Accept();

        var act = () => quote.Reject("Arrependimento");

        act.Should().Throw<QuoteAlreadyDecidedException>()
            .WithMessage(DomainErrorMessages.QuoteAlreadyDecided);
    }

    // ── ServiceOrder.GenerateQuote ──────────────────────────────────────────

    [Fact]
    public void ServiceOrderGenerateQuote_WithActiveServices_CreatesQuote()
    {
        var order = CreateOrderWithConsolidatedService();
        var items = new[] { ValidItem() };

        order.GenerateQuote(items);

        order.Quote.Should().NotBeNull();
        order.Quote!.Status.Should().Be(QuoteStatus.WaitingCustomerApproval);
        order.Quote.Items.Should().HaveCount(1);
    }

    [Fact]
    public void ServiceOrderGenerateQuote_WithActiveServices_SetsStatusToWaitingApproval()
    {
        var order = CreateOrderWithConsolidatedService();

        order.GenerateQuote([ValidItem()]);

        order.Status.Should().Be(ServiceOrderStatus.WaitingApproval);
    }

    [Fact]
    public void ServiceOrderGenerateQuote_WhenQuoteAlreadyExists_ThrowsQuoteAlreadyExistsException()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);

        var act = () => order.GenerateQuote([ValidItem()]);

        act.Should().Throw<QuoteAlreadyExistsException>()
            .WithMessage(DomainErrorMessages.QuoteAlreadyExists);
    }

    [Fact]
    public void ServiceOrderGenerateQuote_WithNoActiveServices_ThrowsNoConsolidatedServicesException()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => order.GenerateQuote([ValidItem()]);

        act.Should().Throw<NoConsolidatedServicesException>()
            .WithMessage(DomainErrorMessages.QuoteNoConsolidatedServices);
    }

    // ── ServiceOrder.AcceptQuote ────────────────────────────────────────────

    [Fact]
    public void ServiceOrderAcceptQuote_WhenPending_AcceptsQuote()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);

        order.AcceptQuote();

        order.Quote!.Status.Should().Be(QuoteStatus.CustomerApproved);
    }

    [Fact]
    public void ServiceOrderAcceptQuote_WhenPending_SetsServiceOrderStatusToQuoteApproved()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);

        order.AcceptQuote();

        order.Status.Should().Be(ServiceOrderStatus.Approved);
    }

    [Fact]
    public void ServiceOrderAcceptQuote_WhenPending_UpdatesUpdatedAt()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);

        order.AcceptQuote();

        order.UpdatedAt.Should().NotBeNull();
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ServiceOrderAcceptQuote_WhenAlreadyAccepted_ThrowsQuoteAlreadyDecidedException()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);
        order.AcceptQuote();

        var act = () => order.AcceptQuote();

        act.Should().Throw<QuoteAlreadyDecidedException>()
            .WithMessage(DomainErrorMessages.ServiceOrderNotWaitingForQuoteApproval);
    }

    [Fact]
    public void ServiceOrderAcceptQuote_WhenAlreadyRejected_ThrowsQuoteAlreadyDecidedException()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);
        order.RejectQuote("Preço alto");

        var act = () => order.AcceptQuote();

        act.Should().Throw<QuoteAlreadyDecidedException>()
            .WithMessage(DomainErrorMessages.ServiceOrderNotWaitingForQuoteApproval);
    }

    [Fact]
    public void ServiceOrderAcceptQuote_WhenNoQuote_ThrowsQuoteNotFoundException()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => order.AcceptQuote();

        act.Should().Throw<QuoteNotFoundException>();
    }

    // ── ServiceOrder.RejectQuote ────────────────────────────────────────────

    [Fact]
    public void ServiceOrderRejectQuote_WhenPendingWithReason_RejectsQuote()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);

        order.RejectQuote("Preço alto");

        order.Quote!.Status.Should().Be(QuoteStatus.CustomerRejected);
        order.Quote.RejectionReason.Should().Be("Preço alto");
    }

    [Fact]
    public void ServiceOrderRejectQuote_WhenPending_SetsServiceOrderStatusToQuoteRejected()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);

        order.RejectQuote("Preço elevado");

        order.Status.Should().Be(ServiceOrderStatus.Rejected);
    }

    [Fact]
    public void ServiceOrderRejectQuote_WhenPending_UpdatesUpdatedAt()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);

        order.RejectQuote("Preço elevado");

        order.UpdatedAt.Should().NotBeNull();
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ServiceOrderRejectQuote_WhenAlreadyRejected_ThrowsQuoteAlreadyDecidedException()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);
        order.RejectQuote("Primeira rejeição");

        var act = () => order.RejectQuote("Segunda rejeição");

        act.Should().Throw<QuoteAlreadyDecidedException>()
            .WithMessage(DomainErrorMessages.ServiceOrderNotWaitingForQuoteApproval);
    }

    [Fact]
    public void ServiceOrderRejectQuote_WhenAlreadyAccepted_ThrowsQuoteAlreadyDecidedException()
    {
        var order = CreateOrderWithConsolidatedService();
        order.GenerateQuote([ValidItem()]);
        order.AcceptQuote();

        var act = () => order.RejectQuote("Arrependimento");

        act.Should().Throw<QuoteAlreadyDecidedException>()
            .WithMessage(DomainErrorMessages.ServiceOrderNotWaitingForQuoteApproval);
    }

    [Fact]
    public void ServiceOrderRejectQuote_WhenNoQuote_ThrowsQuoteNotFoundException()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => order.RejectQuote("Motivo");

        act.Should().Throw<QuoteNotFoundException>();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static ServiceOrder CreateOrderWithConsolidatedService()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var mechanicId = Guid.NewGuid();
        order.StartDiagnostic(mechanicId);
        var serviceId = Guid.NewGuid();
        order.AddDiagnosticService(serviceId);
        order.CompleteDiagnostic("Diagnóstico concluído.");
        order.ConsolidateDiagnosticServices();
        return order;
    }
}
