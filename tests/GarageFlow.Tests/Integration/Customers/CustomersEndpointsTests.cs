using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.Customers.DTOs;
using GarageFlow.Domain.Customers;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Customers;

public sealed class CustomersEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static CreateCustomerRequest ValidRequest(string document = "529.982.247-25") => new(
        "João Silva",
        CustomerDocumentType.Cpf,
        document,
        "joao@email.com",
        "11987654321",
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

    [Fact]
    public async Task PostCustomer_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/customers", ValidRequest("529.982.247-25"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        body!.Id.Should().NotBeEmpty();
        body.IsActive.Should().BeTrue();
        body.Name.Should().Be("João Silva");
    }

    [Fact]
    public async Task GetCustomerById_Existing_Returns200()
    {
        var created = await CreateCustomer("123.456.789-09");

        var response = await _client.GetAsync($"/customers/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetCustomerById_NotFound_Returns404()
    {
        var response = await _client.GetAsync($"/customers/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCustomers_Returns200WithPagination()
    {
        await CreateCustomer("987.654.321-00");

        var response = await _client.GetAsync("/customers?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedCustomerResponse>(JsonOptions);
        body!.Items.Should().NotBeEmpty();
        body.Page.Should().Be(1);
    }

    [Fact]
    public async Task PutCustomer_WithValidData_Returns200()
    {
        var created = await CreateCustomer("111.222.333-96");

        var updateRequest = new UpdateCustomerRequest(
            "João Santos", "joao.santos@email.com", "11912345678",
            "Av. Paulista", "1000", "Ap 1", "Bela Vista", "São Paulo", "SP", "01310100");

        var response = await _client.PutAsJsonAsync($"/customers/{created.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        body!.Name.Should().Be("João Santos");
    }

    [Fact]
    public async Task DeleteCustomer_Returns204AndSoftDeletes()
    {
        var created = await CreateCustomer("444.555.666-19");

        var deleteResponse = await _client.DeleteAsync($"/customers/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/customers/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        body!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCustomer_AlreadyInactive_Returns400()
    {
        var created = await CreateCustomer("555.666.777-20");

        await _client.DeleteAsync($"/customers/{created.Id}");
        var secondDelete = await _client.DeleteAsync($"/customers/{created.Id}");

        secondDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await secondDelete.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(JsonOptions);
        body!.Detail.Should().Be("Cliente já está inativo");
    }

    [Fact]
    public async Task GetCustomers_InvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/customers?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(JsonOptions);
        body!.Status.Should().Be(400);
    }

    [Fact]
    public async Task GetCustomers_InvalidPageSize_Returns400()
    {
        var response = await _client.GetAsync("/customers?page=1&pageSize=-1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(JsonOptions);
        body!.Status.Should().Be(400);
    }

    [Fact]
    public async Task UpdateCustomer_NotFound_Returns404()
    {
        var updateRequest = new UpdateCustomerRequest(
            "João Santos", "joao.santos@email.com", "11912345678",
            "Av. Paulista", "1000", "Ap 1", "Bela Vista", "São Paulo", "SP", "01310100");

        var response = await _client.PutAsJsonAsync($"/customers/{Guid.NewGuid()}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_NotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/customers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostCustomer_DuplicateDocument_Returns409()
    {
        var document = "333.444.555-08";
        await CreateCustomer(document);

        var response = await _client.PostAsJsonAsync("/customers", ValidRequest(document));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<CustomerResponse> CreateCustomer(string document)
    {
        var response = await _client.PostAsJsonAsync("/customers", ValidRequest(document));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions))!;
    }
}
