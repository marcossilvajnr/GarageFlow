using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using GarageFlow.Api.Services.DTOs;
using GarageFlow.Api.Supplies.DTOs;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Services;

public sealed class ServiceSupplyCompositionEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static int _counter;

    private static string UniqueCode(string prefix) =>
        $"{prefix}-{System.Threading.Interlocked.Increment(ref _counter):D3}";

    private async Task<ServiceResponse> CreateServiceAsync()
    {
        var code = UniqueCode("SSC-SRV");
        var request = new CreateServiceRequest(code, $"Serviço {code}", null, 150.00m, 30);
        var response = await _client.PostAsJsonAsync("/services", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions))!;
    }

    private async Task<SupplyResponse> CreateSupplyAsync(string unit = "L")
    {
        var code = UniqueCode("SSC-SUP");
        var request = new CreateSupplyRequest($"Insumo {code}", code, unit, 25.00m, null);
        var response = await _client.PostAsJsonAsync("/supplies", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<SupplyResponse>(JsonOptions))!;
    }

    // ─── POST /services/{id}/supplies: success ───────────────────────────────

    [Fact]
    public async Task PostServiceSupply_WithValidData_Returns200WithSupplies()
    {
        var service = await CreateServiceAsync();
        var supply = await CreateSupplyAsync();

        var response = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(supply.Id, 1.5m));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Supplies.Should().HaveCount(1);
        body.Supplies[0].SupplyId.Should().Be(supply.Id);
        body.Supplies[0].SupplyName.Should().Be(supply.Name);
        body.Supplies[0].Quantity.Should().Be(1.5m);
    }

    // ─── POST /services/{id}/supplies: service not found ────────────────────

    [Fact]
    public async Task PostServiceSupply_WithNonExistentService_Returns404()
    {
        var supply = await CreateSupplyAsync();

        var response = await _client.PostAsJsonAsync(
            $"/services/{Guid.NewGuid()}/supplies",
            new AddServiceSupplyRequest(supply.Id, 1m));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── POST /services/{id}/supplies: supply not found ─────────────────────

    [Fact]
    public async Task PostServiceSupply_WithNonExistentSupply_Returns404()
    {
        var service = await CreateServiceAsync();

        var response = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(Guid.NewGuid(), 1m));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── POST /services/{id}/supplies: invalid quantity ─────────────────────

    [Fact]
    public async Task PostServiceSupply_WithZeroQuantity_Returns400()
    {
        var service = await CreateServiceAsync();
        var supply = await CreateSupplyAsync();

        var response = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(supply.Id, 0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─── POST /services/{id}/supplies: duplicate ─────────────────────────────

    [Fact]
    public async Task PostServiceSupply_WithDuplicateSupply_Returns409()
    {
        var service = await CreateServiceAsync();
        var supply = await CreateSupplyAsync();

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(supply.Id, 1m));

        var response = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(supply.Id, 2m));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ─── DELETE /services/{id}/supplies/{supplyId}: success ─────────────────

    [Fact]
    public async Task DeleteServiceSupply_WithLinkedSupply_Returns204()
    {
        var service = await CreateServiceAsync();
        var supply = await CreateSupplyAsync();

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(supply.Id, 1m));

        var response = await _client.DeleteAsync($"/services/{service.Id}/supplies/{supply.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ─── DELETE /services/{id}/supplies/{supplyId}: service not found ────────

    [Fact]
    public async Task DeleteServiceSupply_WithNonExistentService_Returns404()
    {
        var response = await _client.DeleteAsync(
            $"/services/{Guid.NewGuid()}/supplies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── DELETE /services/{id}/supplies/{supplyId}: supply not linked ────────

    [Fact]
    public async Task DeleteServiceSupply_WithUnlinkedSupply_Returns404()
    {
        var service = await CreateServiceAsync();

        var response = await _client.DeleteAsync(
            $"/services/{service.Id}/supplies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── GET /services/{id}: returns supplies ────────────────────────────────

    [Fact]
    public async Task GetServiceById_WithSupplies_ReturnsSupplies()
    {
        var service = await CreateServiceAsync();
        var supply = await CreateSupplyAsync("KG");

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(supply.Id, 0.5m));

        var response = await _client.GetAsync($"/services/{service.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Supplies.Should().HaveCount(1);
        body.Supplies[0].SupplyId.Should().Be(supply.Id);
        body.Supplies[0].Quantity.Should().Be(0.5m);
    }

    // ─── GET /services: includes supplies ────────────────────────────────────

    [Fact]
    public async Task GetServices_WithSupplies_IncludesSuppliesInItems()
    {
        var service = await CreateServiceAsync();
        var supply = await CreateSupplyAsync("UN");

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(supply.Id, 2m));

        var response = await _client.GetAsync($"/services?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        var target = body!.Items.FirstOrDefault(s => s.Id == service.Id);
        target.Should().NotBeNull();
        target!.Supplies.Should().HaveCount(1);
        target.Supplies[0].SupplyId.Should().Be(supply.Id);
    }

    // ─── GET /services/{id}: returns both parts and supplies ─────────────────

    [Fact]
    public async Task GetServiceById_WithPartsAndSupplies_ReturnsBothCollections()
    {
        var service = await CreateServiceAsync();
        var supply = await CreateSupplyAsync("ML");

        // Add a part
        var partCode = UniqueCode("SSC-PRT");
        var createPartResp = await _client.PostAsJsonAsync("/parts",
            new GarageFlow.Api.Parts.DTOs.CreatePartRequest($"Peça {partCode}", partCode, $"SKU-{partCode}", "UN", 30.00m));
        createPartResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var part = (await createPartResp.Content.ReadFromJsonAsync<GarageFlow.Api.Parts.DTOs.PartResponse>(JsonOptions))!;

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 1));

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/supplies",
            new AddServiceSupplyRequest(supply.Id, 250m));

        var response = await _client.GetAsync($"/services/{service.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Parts.Should().HaveCount(1);
        body.Supplies.Should().HaveCount(1);
    }
}
