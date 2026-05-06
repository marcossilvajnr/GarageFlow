using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Parts;
using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Api.DTOs.Supplies;
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
        var supplyId = await CreateSupply();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(supplyId, StockItemType.Supply, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(supplyId, StockItemType.Supply, 2m, null, null));

        var response = await _client.PostAsJsonAsync("/stock/releases", new ReleaseStockReservationRequest(supplyId, StockItemType.Supply, 1m, "Ajuste manual", "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReleaseStock_WithoutReason_Returns400()
    {
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        var response = await _client.PostAsJsonAsync("/stock/releases", new ReleaseStockReservationRequest(partId, StockItemType.Part, 1m, null, "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReleaseStock_WithoutPerformedBy_Returns400()
    {
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        var response = await _client.PostAsJsonAsync("/stock/releases", new ReleaseStockReservationRequest(partId, StockItemType.Part, 1m, "Ajuste manual", null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReleaseStock_WithFullAuditTrail_PersistsAuditFieldsInOperationLog()
    {
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 15m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 5m, null, null));

        var referenceId = Guid.NewGuid();
        var releaseResponse = await _client.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(partId, StockItemType.Part, 2m, "Cancelamento manual", "gestor.estoque", referenceId, "SeparationOrder"));

        releaseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var opsResponse = await _client.GetAsync($"/stock/{StockItemType.Part}/{partId}/operations?page=1&pageSize=20");
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
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 10m, 0m, null, null));
        await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, null, null));

        var response = await _client.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(partId, StockItemType.Part, 5m, "Ajuste manual", "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ReleaseStock_WithUnknownItem_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/stock/releases",
            new ReleaseStockReservationRequest(Guid.NewGuid(), StockItemType.Part, 1m, "Ajuste", "operador.teste", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListStockOperations_ShouldReturnOperations()
    {
        var partId = await CreatePart();
        await _client.PostAsJsonAsync("/stock/entries", new CreateStockEntryRequest(partId, StockItemType.Part, 12m, 0m, "Entrada", null));
        var reserveResponse = await _client.PostAsJsonAsync("/stock/reservations", new ReserveStockRequest(partId, StockItemType.Part, 2m, "Reserva", null));
        reserveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.GetAsync($"/stock/{StockItemType.Part}/{partId}/operations?page=1&pageSize=20");

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
}
