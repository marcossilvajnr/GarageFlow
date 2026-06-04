using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.Services.DTOs;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Services;

public sealed class ServicesEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static CreateServiceRequest ValidRequest(string code, string name) => new(
        code, name, "Descrição do serviço", 150.00m, 30);

    private async Task<ServiceResponse?> CreateService(string code, string name)
    {
        var response = await _client.PostAsJsonAsync("/services", ValidRequest(code, name));
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"CREATE SERVICE FAILED: Code={code}, Status={response.StatusCode}, Content={errorContent}");
        }
        return await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions);
    }

    [Fact]
    public async Task PostService_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/services", ValidRequest("INT-001", "Serviço Integração 1"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Code.Should().Be("INT-001");
        body.Name.Should().Be("Serviço Integração 1");
        body.IsActive.Should().BeTrue();
        body.BasePrice.Should().Be(150.00m);
    }

    [Fact]
    public async Task PostService_WithZeroBasePrice_Returns400()
    {
        var request = new CreateServiceRequest("INT-002", "Serviço Int 2", null, 0, null);

        var response = await _client.PostAsJsonAsync("/services", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostService_WithDuplicateCode_Returns409()
    {
        await CreateService("INT-003", "Serviço Int 3");

        var response = await _client.PostAsJsonAsync("/services", ValidRequest("INT-003", "Outro Nome"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostService_WithDuplicateName_Returns409()
    {
        await CreateService("INT-004", "Serviço Int 4");

        var response = await _client.PostAsJsonAsync("/services", ValidRequest("INT-005", "Serviço Int 4"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetServiceById_WithExistingId_Returns200()
    {
        var service = await CreateService("INT-006", "Serviço Int 6");
        service.Should().NotBeNull();

        var response = await _client.GetAsync($"/services/{service!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().Be(service.Id);
    }

    [Fact]
    public async Task GetServiceById_WithNonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/services/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetServices_WithValidPagination_Returns200()
    {
        await CreateService("INT-007", "Serviço Int 7");

        var response = await _client.GetAsync("/services?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Items.Should().NotBeEmpty();
        body.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetServices_WithInvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/services?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServices_WithInvalidPageSize_Returns400()
    {
        var response = await _client.GetAsync("/services?page=1&pageSize=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutService_WithValidData_Returns200()
    {
        var service = await CreateService("INT-008", "Serviço Int 8");
        service.Should().NotBeNull();

        var updateRequest = new UpdateServiceRequest("Serviço Int 8 Atualizado", "Nova descrição", 200.00m, 60);

        var response = await _client.PutAsJsonAsync($"/services/{service!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Name.Should().Be("Serviço Int 8 Atualizado");
        body.BasePrice.Should().Be(200.00m);
        body.Code.Should().Be("INT-008");
    }

    [Fact]
    public async Task PutService_WithNonExistentId_Returns404()
    {
        var updateRequest = new UpdateServiceRequest("Nome", null, 100.00m, null);

        var response = await _client.PutAsJsonAsync($"/services/{Guid.NewGuid()}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutService_WithInvalidBasePrice_Returns400()
    {
        var service = await CreateService("INT-009", "Serviço Int 9");
        service.Should().NotBeNull();

        var updateRequest = new UpdateServiceRequest("Serviço Int 9", null, -1, null);

        var response = await _client.PutAsJsonAsync($"/services/{service!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutService_WithDuplicateNameFromOtherService_Returns409()
    {
        await CreateService("INT-010", "Serviço Int 10");
        var second = await CreateService("INT-011", "Serviço Int 11");
        second.Should().NotBeNull();

        var updateRequest = new UpdateServiceRequest("Serviço Int 10", null, 150.00m, null);

        var response = await _client.PutAsJsonAsync($"/services/{second!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteService_WithValidId_Returns204()
    {
        var service = await CreateService("INT-012", "Serviço Int 12");
        service.Should().NotBeNull();

        var response = await _client.DeleteAsync($"/services/{service!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteService_WithNonExistentId_Returns404()
    {
        var response = await _client.DeleteAsync($"/services/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteService_AlreadyInactive_Returns400()
    {
        var service = await CreateService("INT-013", "Serviço Int 13");
        service.Should().NotBeNull();

        await _client.DeleteAsync($"/services/{service!.Id}");
        var secondDelete = await _client.DeleteAsync($"/services/{service.Id}");

        secondDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
