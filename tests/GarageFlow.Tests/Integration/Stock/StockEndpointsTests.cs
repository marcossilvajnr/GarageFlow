using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Employees;
using GarageFlow.Api.DTOs.Executions;
using GarageFlow.Api.DTOs.Parts;
using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Api.DTOs.Supplies;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Employees;
using GarageFlow.Domain.Stock;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Stock;

public sealed class StockEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static int _employeeSeed;
    private static int _cpfSeed = 900_000_000;

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
                $"Employee Stock {seed}",
                CustomerDocumentType.Cpf,
                GenerateValidCpf(),
                $"stock-test-{seed}@garageflow.test",
                $"1196{seed % 1_0000:D4}002",
                "Rua Estoque",
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

    private HttpClient CreateClientWithRole(string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private async Task<Guid> CreatePart()
    {
        var request = new CreatePartRequest("Filtro Ar", $"P-{Guid.NewGuid():N}"[..10], $"SKU-{Guid.NewGuid():N}"[..12], "UN", 50m);
        var response = await _client.PostAsJsonAsync("/parts", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PartResponse>(JsonOptions);
        return body!.Id;
    }

    private async Task<Guid> CreateSupply()
    {
        var request = new CreateSupplyRequest("Óleo", $"S-{Guid.NewGuid():N}"[..10], "L", 25m, null);
        var response = await _client.PostAsJsonAsync("/supplies", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<SupplyResponse>(JsonOptions);
        return body!.Id;
    }

    private async Task<Guid> CreateExecutionOrder()
    {
        var request = new CreateExecutionOrderRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var response = await _client.PostAsJsonAsync("/execution-orders", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        return body!.Id;
    }

    /// <summary>
    /// Creates a SeparationOrder and advances it to Completed status via the full API flow.
    /// Returns (separationOrderId, partId).
    /// </summary>
    private async Task<(Guid SeparationOrderId, Guid PartId)> CreateCompletedSeparationOrderAsync()
    {
        var stockistId = await CreateEmployee(EmployeeRole.Stockist);
        var partId = await CreatePart();
        var partName = "Filtro Ar";

        // Ensure stock exists and has reserved quantity
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 20m, 0m, null, null));

        // Create separation order
        var executionOrderId = await CreateExecutionOrder();
        var createSepReq = new CreateSeparationOrderRequest(
            executionOrderId,
            [new CreateSeparationPartItemRequest(partId, partName, 5)],
            []);
        var createSepResp = await _client.PostAsJsonAsync("/separation-orders", createSepReq);
        createSepResp.EnsureSuccessStatusCode();
        var sepBody = await createSepResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions);
        var separationOrderId = sepBody!.Id;

        // Reserve separation order (also reserves stock)
        var reserveResp = await _client.PostAsJsonAsync($"/separation-orders/{separationOrderId}/reserve", new { });
        reserveResp.EnsureSuccessStatusCode();

        // Confirm stockist withdrawal
        var stockistResp = await _client.PostAsJsonAsync(
            $"/separation-orders/{separationOrderId}/confirm-stockist-withdrawal",
            new ConfirmSeparationStockistWithdrawalRequest(stockistId));
        stockistResp.EnsureSuccessStatusCode();

        // Confirm mechanic receipt → Status = Completed
        var mechanicResp = await _client.PostAsJsonAsync($"/separation-orders/{separationOrderId}/confirm-mechanic-receipt", new { });
        mechanicResp.EnsureSuccessStatusCode();

        return (separationOrderId, partId);
    }

    [Fact]
    public async Task CreateStockEntry_WithValidPart_Returns200()
    {
        var partId = await CreatePart();
        var request = new CreateStockEntryRequest(partId, StockItemType.Part, 20m, 5m, "Entrada inicial", null);

        var response = await _client.PostAsJsonAsync("/stock/entries", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StockPositionResponse>(JsonOptions);
        body!.TotalQuantity.Should().Be(20m);
        body.AvailableQuantity.Should().Be(20m);
    }

    [Fact]
    public async Task ReserveStock_WithInsufficientAvailability_Returns409()
    {
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 2m, 0m, null, null));

        var response = await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 5m, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConsumeStock_FullFlow_ShouldUpdatePosition()
    {
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        var reserveResponse = await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 4m, null, null));
        reserveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var consumeResponse = await _client.PostAsJsonAsync("/stock/consumptions", new ConsumeStockRequest(partId, StockItemType.Part, 3m, null, null));
        consumeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var positionResponse = await _client.GetAsync($"/stock/{StockItemType.Part}/{partId}");
        positionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var position = await positionResponse.Content.ReadFromJsonAsync<StockPositionResponse>(JsonOptions);

        position!.TotalQuantity.Should().Be(7m);
        position.ReservedQuantity.Should().Be(1m);
        position.AvailableQuantity.Should().Be(6m);
    }

    [Fact]
    public async Task ReleaseStock_ForSupply_WithReason_Returns200()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var supplyId = await CreateSupply();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(supplyId, StockItemType.Supply, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(supplyId, StockItemType.Supply, 2m, null, null));

        var response = await adminClient.PostAsJsonAsync("/stock/releases", new ReleaseStockReservationRequest(supplyId, StockItemType.Supply, 1m, "Ajuste manual", "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReleaseStock_WithoutReason_Returns400()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        var response = await adminClient.PostAsJsonAsync("/stock/releases", new ReleaseStockReservationRequest(partId, StockItemType.Part, 1m, null, "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReleaseStock_WithoutPerformedBy_Returns400()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        var response = await adminClient.PostAsJsonAsync("/stock/releases", new ReleaseStockReservationRequest(partId, StockItemType.Part, 1m, "Ajuste manual", null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReleaseStock_WithFullAuditTrail_PersistsAuditFieldsInOperationLog()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 15m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 5m, null, null));

        var referenceId = Guid.NewGuid();
        var releaseResponse = await adminClient.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(partId, StockItemType.Part, 2m, "Cancelamento manual", "gestor.estoque", referenceId, "SeparationOrder"));

        releaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var opsClient = CreateClientWithRole("Administrative");
        var opsResponse = await opsClient.GetAsync($"/stock/{StockItemType.Part}/{partId}/operations?page=1&pageSize=20");
        opsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await opsResponse.Content.ReadFromJsonAsync<PagedStockOperationsResponse>(JsonOptions);

        var releaseOp = body!.Items.Single(o => o.Type == StockOperationType.Release);
        releaseOp.PerformedBy.Should().Be("gestor.estoque");
        releaseOp.Reason.Should().Be("Cancelamento manual");
        releaseOp.ReferenceId.Should().Be(referenceId);
        releaseOp.ReferenceType.Should().Be("SeparationOrder");
    }

    [Fact]
    public async Task ReleaseStock_WithExcessQuantity_Returns409()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        var response = await adminClient.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(partId, StockItemType.Part, 5m, "Ajuste manual", "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ReleaseStock_WithUnknownItem_Returns404()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var response = await adminClient.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(Guid.NewGuid(), StockItemType.Part, 1m, "Ajuste", "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReleaseStock_WithoutToken_Returns401()
    {
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        var anonymousClient = factory.CreateClient();
        anonymousClient.DefaultRequestHeaders.Add(TestAuthHandler.ForceAnonymousHeader, "true");

        var response = await anonymousClient.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(partId, StockItemType.Part, 1m, "Ajuste manual", "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReleaseStock_WithNonAdminRole_Returns403()
    {
        var stockistClient = CreateClientWithRole("Stockist");
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        var response = await stockistClient.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(partId, StockItemType.Part, 1m, "Ajuste manual", "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReleaseStock_WithAdministrativeRole_Returns200()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 3m, null, null));

        var response = await adminClient.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(partId, StockItemType.Part, 2m, "Liberação administrativa", "admin.usuario", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListStockOperations_ShouldReturnOperations()
    {
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 12m, 0m, "Entrada", null));
        var reserveResponse = await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, "Reserva", null));
        reserveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var adminClient = CreateClientWithRole("Administrative");
        var response = await adminClient.GetAsync($"/stock/{StockItemType.Part}/{partId}/operations?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedStockOperationsResponse>(JsonOptions);
        body!.Items.Should().HaveCount(2);
        body.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task CreateStockEntry_WithUnknownItem_Returns404()
    {
        var response = await _client.PostAsJsonAsync(
            "/stock/entries",
            new CreateStockEntryRequest(Guid.NewGuid(), StockItemType.Part, 10m, 0m, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListStockOperations_WithoutToken_Returns401()
    {
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 8m, 0m, "Entrada", null));

        var anonymousClient = factory.CreateClient();
        anonymousClient.DefaultRequestHeaders.Add(TestAuthHandler.ForceAnonymousHeader, "true");

        var response = await anonymousClient.GetAsync($"/stock/{StockItemType.Part}/{partId}/operations?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ---------------------------------------------------------------------------
    // Task-033: post-custody exceptional adjustment — integration tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ReleaseStock_PostCustody_WithoutReference_Returns400()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var (_, partId) = await CreateCompletedSeparationOrderAsync();

        // The separation order reserved 5 units; add more reserved quantity to release
        await _client.PostAsJsonAsync("/stock/reservations",
            new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        // Attempt release without referenceId/referenceType — must fail because item is post-custody
        var response = await adminClient.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(partId, StockItemType.Part, 1m, "Ajuste excepcional", "admin.user", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReleaseStock_PostCustody_WithValidReference_Returns200()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var (separationOrderId, partId) = await CreateCompletedSeparationOrderAsync();

        // Add more reserved quantity to release (the separation order reserved 5 units)
        await _client.PostAsJsonAsync("/stock/reservations",
            new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        // Exceptional post-custody adjustment with all mandatory fields
        var response = await adminClient.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(
                partId, StockItemType.Part, 2m,
                "Ajuste excepcional pós-custódia autorizado",
                "admin.user",
                separationOrderId, "SeparationOrder"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StockPositionResponse>(JsonOptions);
        body!.AvailableQuantity.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task ReturnSeparationOrderTotal_AfterMechanicReceipt_Returns400()
    {
        // RN-032: operational return must be blocked after ConfirmMechanicReceipt
        var (separationOrderId, _) = await CreateCompletedSeparationOrderAsync();

        var response = await _client.PostAsJsonAsync(
            $"/separation-orders/{separationOrderId}/return-total", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReleaseStock_PostCustody_WithUnsupportedReferenceType_Returns400()
    {
        var adminClient = CreateClientWithRole("Administrative");
        var (separationOrderId, partId) = await CreateCompletedSeparationOrderAsync();

        await _client.PostAsJsonAsync("/stock/reservations",
            new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        var response = await adminClient.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(
                partId, StockItemType.Part, 1m,
                "Ajuste excepcional pós-custódia",
                "admin.user",
                separationOrderId, "ServiceOrder"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
