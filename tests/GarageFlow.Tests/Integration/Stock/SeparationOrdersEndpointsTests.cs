using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Stock;

public sealed class SeparationOrdersEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static CreateSeparationOrderRequest ValidCreateRequest(Guid? executionOrderId = null) =>
        new(
            executionOrderId ?? Guid.NewGuid(),
            [new CreateSeparationPartItemRequest(Guid.NewGuid(), "Filtro de óleo", 2)],
            []);

    private async Task<SeparationOrderResponse> CreateSeparationOrder(CreateSeparationOrderRequest? request = null)
    {
        var req = request ?? ValidCreateRequest();
        var response = await _client.PostAsJsonAsync("/separation-orders", req);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;
    }

    // --- POST /separation-orders ---

    [Fact]
    public async Task CreateSeparationOrder_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/separation-orders", ValidCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Status.Should().Be(SeparationOrderStatus.Pending);
        body.Parts.Should().HaveCount(1);
        body.Supplies.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateSeparationOrder_WithEmptyExecutionOrderId_Returns400()
    {
        var request = ValidCreateRequest(Guid.Empty);
        var response = await _client.PostAsJsonAsync("/separation-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSeparationOrder_WithNoItems_Returns400()
    {
        var request = new CreateSeparationOrderRequest(Guid.NewGuid(), [], []);
        var response = await _client.PostAsJsonAsync("/separation-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSeparationOrder_WithNullLists_Returns400()
    {
        var request = new CreateSeparationOrderRequest(Guid.NewGuid(), null, null);
        var response = await _client.PostAsJsonAsync("/separation-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- GET /separation-orders/{id} ---

    [Fact]
    public async Task GetSeparationOrderById_WhenExists_Returns200()
    {
        var created = await CreateSeparationOrder();

        var response = await _client.GetAsync($"/separation-orders/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetSeparationOrderById_WhenNotExists_Returns404()
    {
        var response = await _client.GetAsync($"/separation-orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- GET /separation-orders ---

    [Fact]
    public async Task ListSeparationOrders_Returns200()
    {
        var response = await _client.GetAsync("/separation-orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListSeparationOrders_WithInvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/separation-orders?page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- POST /separation-orders/{id}/reserve ---

    [Fact]
    public async Task Reserve_WhenPending_Returns200WithWaitingPickup()
    {
        var created = await CreateSeparationOrder();

        var response = await _client.PostAsync($"/separation-orders/{created.Id}/reserve", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        body!.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    [Fact]
    public async Task Reserve_WhenAlreadyReserved_Returns409()
    {
        var created = await CreateSeparationOrder();
        await _client.PostAsync($"/separation-orders/{created.Id}/reserve", null);

        var response = await _client.PostAsync($"/separation-orders/{created.Id}/reserve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Reserve_WhenNotFound_Returns404()
    {
        var response = await _client.PostAsync($"/separation-orders/{Guid.NewGuid()}/reserve", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- POST /separation-orders/{id}/wait-purchase ---

    [Fact]
    public async Task WaitPurchase_WhenPending_Returns200WithWaitingPurchase()
    {
        var created = await CreateSeparationOrder();

        var response = await _client.PostAsync($"/separation-orders/{created.Id}/wait-purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        body!.Status.Should().Be(SeparationOrderStatus.WaitingPurchase);
    }

    [Fact]
    public async Task WaitPurchase_WhenNotPending_Returns409()
    {
        var created = await CreateSeparationOrder();
        await _client.PostAsync($"/separation-orders/{created.Id}/reserve", null);

        var response = await _client.PostAsync($"/separation-orders/{created.Id}/wait-purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task WaitPurchase_WhenNotFound_Returns404()
    {
        var response = await _client.PostAsync($"/separation-orders/{Guid.NewGuid()}/wait-purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- POST /separation-orders/{id}/resume-after-purchase ---

    [Fact]
    public async Task ResumeAfterPurchase_WhenWaitingPurchase_Returns200WithWaitingPickup()
    {
        var created = await CreateSeparationOrder();
        await _client.PostAsync($"/separation-orders/{created.Id}/wait-purchase", null);

        var response = await _client.PostAsync($"/separation-orders/{created.Id}/resume-after-purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        body!.Status.Should().Be(SeparationOrderStatus.WaitingPickup);
    }

    [Fact]
    public async Task ResumeAfterPurchase_WhenNotWaitingPurchase_Returns409()
    {
        var created = await CreateSeparationOrder();

        var response = await _client.PostAsync($"/separation-orders/{created.Id}/resume-after-purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ResumeAfterPurchase_WhenNotFound_Returns404()
    {
        var response = await _client.PostAsync($"/separation-orders/{Guid.NewGuid()}/resume-after-purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- POST /separation-orders/{id}/confirm-stockist-withdrawal ---

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenWaitingPickup_Returns200WithSeparated()
    {
        var created = await CreateSeparationOrder();
        await _client.PostAsync($"/separation-orders/{created.Id}/reserve", null);

        var request = new ConfirmSeparationStockistWithdrawalRequest(Guid.NewGuid());
        var response = await _client.PostAsJsonAsync($"/separation-orders/{created.Id}/confirm-stockist-withdrawal", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        body!.Status.Should().Be(SeparationOrderStatus.Separated);
    }

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenNotWaitingPickup_Returns409()
    {
        var created = await CreateSeparationOrder();

        var request = new ConfirmSeparationStockistWithdrawalRequest(Guid.NewGuid());
        var response = await _client.PostAsJsonAsync($"/separation-orders/{created.Id}/confirm-stockist-withdrawal", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConfirmStockistWithdrawal_WithEmptyStockistId_Returns400()
    {
        var created = await CreateSeparationOrder();
        await _client.PostAsync($"/separation-orders/{created.Id}/reserve", null);

        var request = new ConfirmSeparationStockistWithdrawalRequest(Guid.Empty);
        var response = await _client.PostAsJsonAsync($"/separation-orders/{created.Id}/confirm-stockist-withdrawal", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmStockistWithdrawal_WhenNotFound_Returns404()
    {
        var request = new ConfirmSeparationStockistWithdrawalRequest(Guid.NewGuid());
        var response = await _client.PostAsJsonAsync($"/separation-orders/{Guid.NewGuid()}/confirm-stockist-withdrawal", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- POST /separation-orders/{id}/confirm-mechanic-receipt ---

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenSeparated_Returns200WithCompleted()
    {
        var created = await CreateSeparationOrder();
        await _client.PostAsync($"/separation-orders/{created.Id}/reserve", null);
        await _client.PostAsJsonAsync($"/separation-orders/{created.Id}/confirm-stockist-withdrawal",
            new ConfirmSeparationStockistWithdrawalRequest(Guid.NewGuid()));

        var response = await _client.PostAsync($"/separation-orders/{created.Id}/confirm-mechanic-receipt", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        body!.Status.Should().Be(SeparationOrderStatus.Completed);
    }

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenNotSeparated_Returns409()
    {
        var created = await CreateSeparationOrder();
        await _client.PostAsync($"/separation-orders/{created.Id}/reserve", null);

        var response = await _client.PostAsync($"/separation-orders/{created.Id}/confirm-mechanic-receipt", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenNotFound_Returns404()
    {
        var response = await _client.PostAsync($"/separation-orders/{Guid.NewGuid()}/confirm-mechanic-receipt", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
