using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Employees;
using GarageFlow.Api.DTOs.Executions;
using GarageFlow.Api.DTOs.Parts;
using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Stock;
using GarageFlow.Infrastructure.Persistence;
using GarageFlow.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Tests.Integration.Executions;

public sealed class ExecutionOrdersEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static int _employeeSeed;
    private static int _cpfSeed = 700_000_000;

    private static string GenerateValidCpf()
    {
        var baseDigits = Interlocked.Increment(ref _cpfSeed) % 1_000_000_000;
        var baseNumber = baseDigits.ToString("D9");
        var firstDigit = CalculateCpfVerifier(baseNumber, 10);
        var secondDigit = CalculateCpfVerifier(baseNumber + firstDigit, 11);
        var rawCpf = $"{baseNumber}{firstDigit}{secondDigit}";
        return $"{rawCpf[..3]}.{rawCpf.Substring(3, 3)}.{rawCpf.Substring(6, 3)}-{rawCpf.Substring(9, 2)}";
    }

    private static int CalculateCpfVerifier(string digits, int weightStart)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            sum += (digits[i] - '0') * (weightStart - i);
        }

        var mod = sum % 11;
        return mod < 2 ? 0 : 11 - mod;
    }

    private async Task<Guid> CreateEmployee(EmployeeRole role)
    {
        var seed = Interlocked.Increment(ref _employeeSeed);
        var response = await _client.PostAsJsonAsync(
            "/employees",
            new CreateEmployeeRequest(
                $"Employee Execution {seed}",
                CustomerDocumentType.Cpf,
                GenerateValidCpf(),
                $"execution-test-{seed}@garageflow.test",
                $"1198{seed % 1_0000:D4}000",
                "Rua Execucao",
                "10",
                null,
                "Centro",
                "Sao Paulo",
                "SP",
                "01310100",
                role));
        response.EnsureSuccessStatusCode();
        var employee = await response.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions);
        return employee!.Id;
    }

    private async Task<Guid> SeedServiceOrderInExecution()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
        var so = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        typeof(ServiceOrder)
            .GetProperty(nameof(ServiceOrder.Status))!
            .SetValue(so, ServiceOrderStatus.InExecution);
        db.ServiceOrders.Add(so);
        await db.SaveChangesAsync();
        return so.Id;
    }

    private async Task<Guid> SeedServiceOrderApproved()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
        var so = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        typeof(ServiceOrder)
            .GetProperty(nameof(ServiceOrder.Status))!
            .SetValue(so, ServiceOrderStatus.Approved);
        db.ServiceOrders.Add(so);
        await db.SaveChangesAsync();
        return so.Id;
    }

    private static CreateExecutionOrderRequest ValidCreateRequest(
        Guid? serviceOrderId = null,
        Guid? serviceId = null,
        Guid? mechanicId = null) =>
        new(serviceOrderId ?? Guid.NewGuid(), serviceId ?? Guid.NewGuid(), mechanicId ?? Guid.NewGuid());

    private async Task<ExecutionOrderResponse> CreateExecutionOrder(CreateExecutionOrderRequest? request = null)
    {
        var req = request;
        if (req is null)
        {
            var serviceOrderId = await SeedServiceOrderApproved();
            req = ValidCreateRequest(serviceOrderId);
        }
        var response = await _client.PostAsJsonAsync("/execution-orders", req);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions))!;
    }

    // --- POST /execution-orders ---

    [Fact]
    public async Task CreateExecutionOrder_WithValidData_Returns201()
    {
        var request = ValidCreateRequest();
        var response = await _client.PostAsJsonAsync("/execution-orders", request);
        var body = await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().NotBeNull();
        body!.Status.Should().Be(ExecutionOrderStatus.Pending);
        body.MechanicId.Should().Be(request.MechanicId);
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

    [Fact]
    public async Task CreateExecutionOrder_WithEmptyMechanicId_Returns400()
    {
        var request = ValidCreateRequest(mechanicId: Guid.Empty);
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
        var response = await _client.PostAsync($"/execution-orders/{created.Id}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        body!.Status.Should().Be(ExecutionOrderStatus.InExecution);
        body.MechanicId.Should().NotBeNull();
        body.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task StartExecution_WhenPending_Returns409()
    {
        var created = await CreateExecutionOrder();
        var response = await _client.PostAsync($"/execution-orders/{created.Id}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task StartExecution_WhenNotFound_Returns404()
    {
        var response = await _client.PostAsync($"/execution-orders/{Guid.NewGuid()}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- POST /execution-orders/{id}/complete ---

    private async Task<Guid> CreatePartWithStock(decimal quantity = 100m)
    {
        var partCode = $"P-{Guid.NewGuid():N}"[..10];
        var partSku = $"SKU-{Guid.NewGuid():N}"[..12];
        var partResp = await _client.PostAsJsonAsync("/parts", new CreatePartRequest("Filtro de óleo", partCode, partSku, "UN", 50m));
        partResp.EnsureSuccessStatusCode();
        var part = (await partResp.Content.ReadFromJsonAsync<PartResponse>(JsonOptions))!;

        var stockResp = await _client.PostAsJsonAsync("/stock/entries",
            new CreateStockEntryRequest(part.Id, StockItemType.Part, quantity, 0m, "Seed integração execução", null));
        stockResp.EnsureSuccessStatusCode();

        return part.Id;
    }

    private async Task<SeparationOrderResponse> CreateAndReserveSeparation(Guid executionOrderId, Guid partId)
    {
        var createResp = await _client.PostAsJsonAsync("/separation-orders",
            new CreateSeparationOrderRequest(
                executionOrderId,
                [new CreateSeparationPartItemRequest(partId, "Filtro de óleo", 1)],
                []));
        createResp.EnsureSuccessStatusCode();
        var separation = (await createResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;

        var reserveResp = await _client.PostAsync($"/separation-orders/{separation.Id}/reserve", null);
        reserveResp.EnsureSuccessStatusCode();
        await MarkSeparationAsCompleted(separation.Id);

        return separation;
    }

    private async Task MarkSeparationAsCompleted(Guid separationOrderId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
        var separation = await db.SeparationOrders.FindAsync(separationOrderId);
        separation.Should().NotBeNull();
        typeof(SeparationOrder)
            .GetProperty(nameof(SeparationOrder.Status))!
            .SetValue(separation!, SeparationOrderStatus.Completed);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CompleteExecution_WhenInExecution_Returns200WithCompleted()
    {
        var serviceOrderId = await SeedServiceOrderInExecution();
        var created = await CreateExecutionOrder(new CreateExecutionOrderRequest(serviceOrderId, Guid.NewGuid(), Guid.NewGuid()));
        var partId = await CreatePartWithStock();
        await CreateAndReserveSeparation(created.Id, partId);
        await _client.PostAsync($"/execution-orders/{created.Id}/mark-ready", null);
        await _client.PostAsync($"/execution-orders/{created.Id}/start", null);

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
        var partId = await CreatePartWithStock();
        await CreateAndReserveSeparation(created.Id, partId);

        var response = await _client.PostAsync($"/execution-orders/{created.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CompleteExecution_WhenReady_Returns409()
    {
        var created = await CreateExecutionOrder();
        var partId = await CreatePartWithStock();
        await CreateAndReserveSeparation(created.Id, partId);
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
