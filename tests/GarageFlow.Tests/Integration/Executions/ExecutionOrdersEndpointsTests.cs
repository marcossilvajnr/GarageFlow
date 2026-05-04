using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Executions;
using GarageFlow.Domain.Executions;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Executions;

public sealed class ExecutionOrdersEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static CreateExecutionOrderRequest ValidCreateRequest(
        Guid? serviceOrderId = null,
        Guid? serviceId = null) =>
        new(serviceOrderId ?? Guid.NewGuid(), serviceId ?? Guid.NewGuid());

    private async Task<ExecutionOrderResponse> CreateExecutionOrder(CreateExecutionOrderRequest? request = null)
    {
        var req = request ?? ValidCreateRequest();
        var response = await _client.PostAsJsonAsync("/execution-orders", req);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions))!;
    }

    // --- POST /execution-orders ---

    [Fact]
    public async Task CreateExecutionOrder_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/execution-orders", ValidCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Status.Should().Be(ExecutionOrderStatus.Pending);
        body.MechanicId.Should().BeNull();
        body.StartedAt.Should().BeNull();
        body.CompletedAt.Should().BeNull();
        body.ActualTimeMinutes.Should().BeNull();
    }

    [Fact]
    public async Task CreateExecutionOrder_WithEmptyServiceOrderId_Returns400()
    {
        var request = ValidCreateRequest(serviceOrderId: Guid.Empty);
        var response = await _client.PostAsJsonAsync("/execution-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateExecutionOrder_WithEmptyServiceId_Returns400()
    {
        var request = ValidCreateRequest(serviceId: Guid.Empty);
        var response = await _client.PostAsJsonAsync("/execution-orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- GET /execution-orders/{id} ---

    [Fact]
    public async Task GetExecutionOrderById_WhenExists_Returns200()
    {
        var created = await CreateExecutionOrder();

        var response = await _client.GetAsync($"/execution-orders/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetExecutionOrderById_WhenNotExists_Returns404()
    {
        var response = await _client.GetAsync($"/execution-orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- GET /execution-orders ---

    [Fact]
    public async Task ListExecutionOrders_Returns200()
    {
        var response = await _client.GetAsync("/execution-orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListExecutionOrders_WithInvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/execution-orders?page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListExecutionOrders_WithPageSizeAboveMax_Returns400()
    {
        var response = await _client.GetAsync("/execution-orders?page=1&pageSize=101");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- POST /execution-orders/{id}/mark-ready ---

    [Fact]
    public async Task MarkReady_WhenPending_Returns200WithReadyStatus()
    {
        var created = await CreateExecutionOrder();

        var response = await _client.PostAsync($"/execution-orders/{created.Id}/mark-ready", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        body!.Status.Should().Be(ExecutionOrderStatus.Ready);
    }

    [Fact]
    public async Task MarkReady_CalledTwice_IsIdempotent()
    {
        var created = await CreateExecutionOrder();
        await _client.PostAsync($"/execution-orders/{created.Id}/mark-ready", null);

        var response = await _client.PostAsync($"/execution-orders/{created.Id}/mark-ready", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        body!.Status.Should().Be(ExecutionOrderStatus.Ready);
    }

    [Fact]
    public async Task MarkReady_WhenNotFound_Returns404()
    {
        var response = await _client.PostAsync($"/execution-orders/{Guid.NewGuid()}/mark-ready", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- POST /execution-orders/{id}/start ---

    [Fact]
    public async Task StartExecution_WhenReady_Returns200WithInExecution()
    {
        var created = await CreateExecutionOrder();
        await _client.PostAsync($"/execution-orders/{created.Id}/mark-ready", null);

        var request = new StartExecutionOrderRequest(Guid.NewGuid());
        var response = await _client.PostAsJsonAsync($"/execution-orders/{created.Id}/start", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        body!.Status.Should().Be(ExecutionOrderStatus.InExecution);
        body.MechanicId.Should().Be(request.MechanicId);
        body.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task StartExecution_WhenPending_Returns409()
    {
        var created = await CreateExecutionOrder();

        var request = new StartExecutionOrderRequest(Guid.NewGuid());
        var response = await _client.PostAsJsonAsync($"/execution-orders/{created.Id}/start", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task StartExecution_WithEmptyMechanicId_Returns400()
    {
        var created = await CreateExecutionOrder();
        await _client.PostAsync($"/execution-orders/{created.Id}/mark-ready", null);

        var request = new StartExecutionOrderRequest(Guid.Empty);
        var response = await _client.PostAsJsonAsync($"/execution-orders/{created.Id}/start", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StartExecution_WhenNotFound_Returns404()
    {
        var request = new StartExecutionOrderRequest(Guid.NewGuid());
        var response = await _client.PostAsJsonAsync($"/execution-orders/{Guid.NewGuid()}/start", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- POST /execution-orders/{id}/complete ---

    [Fact]
    public async Task CompleteExecution_WhenInExecution_Returns200WithCompleted()
    {
        var created = await CreateExecutionOrder();
        await _client.PostAsync($"/execution-orders/{created.Id}/mark-ready", null);
        await _client.PostAsJsonAsync($"/execution-orders/{created.Id}/start", new StartExecutionOrderRequest(Guid.NewGuid()));

        var response = await _client.PostAsync($"/execution-orders/{created.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        body!.Status.Should().Be(ExecutionOrderStatus.Completed);
        body.CompletedAt.Should().NotBeNull();
        body.ActualTimeMinutes.Should().NotBeNull();
        body.ActualTimeMinutes!.Value.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task CompleteExecution_WhenPending_Returns409()
    {
        var created = await CreateExecutionOrder();

        var response = await _client.PostAsync($"/execution-orders/{created.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CompleteExecution_WhenReady_Returns409()
    {
        var created = await CreateExecutionOrder();
        await _client.PostAsync($"/execution-orders/{created.Id}/mark-ready", null);

        var response = await _client.PostAsync($"/execution-orders/{created.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CompleteExecution_WhenNotFound_Returns404()
    {
        var response = await _client.PostAsync($"/execution-orders/{Guid.NewGuid()}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
