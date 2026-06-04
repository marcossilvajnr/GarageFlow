using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.Parts.DTOs;
using GarageFlow.Api.Services.DTOs;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Services;

public sealed class ServicePartCompositionEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static int _counter;

    private static string UniqueCode(string prefix) =>
        $"{prefix}-{System.Threading.Interlocked.Increment(ref _counter):D3}";

    private async Task<ServiceResponse> CreateServiceAsync()
    {
        var code = UniqueCode("SPC-SRV");
        var request = new CreateServiceRequest(code, $"Serviço {code}", null, 150.00m, 30);
        var response = await _client.PostAsJsonAsync("/services", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions))!;
    }

    private async Task<PartResponse> CreatePartAsync()
    {
        var code = UniqueCode("SPC-PRT");
        var request = new CreatePartRequest($"Peça {code}", code, $"SKU-{code}", "UN", 25.00m);
        var response = await _client.PostAsJsonAsync("/parts", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<PartResponse>(JsonOptions))!;
    }

    // ─── POST /services/{id}/parts: success ──────────────────────────────────

    [Fact]
    public async Task PostServicePart_WithValidData_Returns200WithParts()
    {
        var service = await CreateServiceAsync();
        var part = await CreatePartAsync();

        var response = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 2));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Parts.Should().HaveCount(1);
        body.Parts[0].PartId.Should().Be(part.Id);
        body.Parts[0].PartName.Should().Be(part.Name);
        body.Parts[0].Quantity.Should().Be(2);
    }

    // ─── POST /services/{id}/parts: service not found ────────────────────────

    [Fact]
    public async Task PostServicePart_WithNonExistentService_Returns404()
    {
        var part = await CreatePartAsync();

        var response = await _client.PostAsJsonAsync(
            $"/services/{Guid.NewGuid()}/parts",
            new AddServicePartRequest(part.Id, 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── POST /services/{id}/parts: part not found ───────────────────────────

    [Fact]
    public async Task PostServicePart_WithNonExistentPart_Returns404()
    {
        var service = await CreateServiceAsync();

        var response = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(Guid.NewGuid(), 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── POST /services/{id}/parts: invalid quantity ─────────────────────────

    [Fact]
    public async Task PostServicePart_WithZeroQuantity_Returns400()
    {
        var service = await CreateServiceAsync();
        var part = await CreatePartAsync();

        var response = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─── POST /services/{id}/parts: duplicate ────────────────────────────────

    [Fact]
    public async Task PostServicePart_WithDuplicatePart_Returns409()
    {
        var service = await CreateServiceAsync();
        var part = await CreatePartAsync();

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 1));

        var response = await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 2));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ─── DELETE /services/{id}/parts/{partId}: success ───────────────────────

    [Fact]
    public async Task DeleteServicePart_WithLinkedPart_Returns204()
    {
        var service = await CreateServiceAsync();
        var part = await CreatePartAsync();

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 1));

        var response = await _client.DeleteAsync($"/services/{service.Id}/parts/{part.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ─── DELETE /services/{id}/parts/{partId}: service not found ────────────

    [Fact]
    public async Task DeleteServicePart_WithNonExistentService_Returns404()
    {
        var response = await _client.DeleteAsync($"/services/{Guid.NewGuid()}/parts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── DELETE /services/{id}/parts/{partId}: part not linked ──────────────

    [Fact]
    public async Task DeleteServicePart_WithUnlinkedPart_Returns404()
    {
        var service = await CreateServiceAsync();

        var response = await _client.DeleteAsync($"/services/{service.Id}/parts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── GET /services/{id}: returns parts ───────────────────────────────────

    [Fact]
    public async Task GetServiceById_WithParts_ReturnsParts()
    {
        var service = await CreateServiceAsync();
        var part = await CreatePartAsync();

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 3));

        var response = await _client.GetAsync($"/services/{service.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Parts.Should().HaveCount(1);
        body.Parts[0].PartId.Should().Be(part.Id);
        body.Parts[0].Quantity.Should().Be(3);
    }

    // ─── GET /services: includes parts ───────────────────────────────────────

    [Fact]
    public async Task GetServices_WithParts_IncludesPartsInItems()
    {
        var service = await CreateServiceAsync();
        var part = await CreatePartAsync();

        await _client.PostAsJsonAsync(
            $"/services/{service.Id}/parts",
            new AddServicePartRequest(part.Id, 1));

        var response = await _client.GetAsync($"/services?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        var created = body!.Items.FirstOrDefault(s => s.Id == service.Id);
        created.Should().NotBeNull();
        created!.Parts.Should().HaveCount(1);
        created.Parts[0].PartId.Should().Be(part.Id);
    }
}
