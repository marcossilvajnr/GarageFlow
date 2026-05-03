using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Employees;
using GarageFlow.Domain.Employees;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Employees;

public sealed class EmployeesEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static CreateEmployeeRequest ValidRequest(string document = "529.982.247-25") => new(
        "Maria Silva",
        GarageFlow.Domain.Customers.CustomerDocumentType.Cpf,
        document,
        "maria@email.com",
        "11987654321",
        "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100",
        EmployeeRole.Mechanic);

    [Fact]
    public async Task PostEmployee_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/employees", ValidRequest("529.982.247-25"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions);
        body!.Id.Should().NotBeEmpty();
        body.IsActive.Should().BeTrue();
        body.Name.Should().Be("Maria Silva");
        body.Role.Should().Be(EmployeeRole.Mechanic);
    }

    [Fact]
    public async Task GetEmployeeById_Existing_Returns200()
    {
        var created = await CreateEmployee("123.456.789-09");

        var response = await _client.GetAsync($"/employees/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions);
        body!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetEmployeeById_NotFound_Returns404()
    {
        var response = await _client.GetAsync($"/employees/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEmployees_Returns200WithPagination()
    {
        await CreateEmployee("987.654.321-00");

        var response = await _client.GetAsync("/employees?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedEmployeeResponse>(JsonOptions);
        body!.Items.Should().NotBeEmpty();
        body.Page.Should().Be(1);
    }

    [Fact]
    public async Task PutEmployee_WithValidData_Returns200()
    {
        var created = await CreateEmployee("111.222.333-96");

        var updateRequest = new UpdateEmployeeRequest(
            "Maria Santos", "maria.santos@email.com", "11912345678",
            "Av. Paulista", "1000", "Ap 1", "Bela Vista", "São Paulo", "SP", "01310100",
            EmployeeRole.Stockist);

        var response = await _client.PutAsJsonAsync($"/employees/{created.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions);
        body!.Name.Should().Be("Maria Santos");
        body.Role.Should().Be(EmployeeRole.Stockist);
    }

    [Fact]
    public async Task DeleteEmployee_Returns204AndSoftDeletes()
    {
        var created = await CreateEmployee("444.555.666-19");

        var deleteResponse = await _client.DeleteAsync($"/employees/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/employees/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResponse.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions);
        body!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEmployee_AlreadyInactive_Returns400()
    {
        var created = await CreateEmployee("555.666.777-20");

        await _client.DeleteAsync($"/employees/{created.Id}");
        var secondDelete = await _client.DeleteAsync($"/employees/{created.Id}");

        secondDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await secondDelete.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(JsonOptions);
        body!.Detail.Should().Be("Funcionário já está inativo");
    }

    [Fact]
    public async Task GetEmployees_InvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/employees?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(JsonOptions);
        body!.Status.Should().Be(400);
    }

    [Fact]
    public async Task GetEmployees_InvalidPageSize_Returns400()
    {
        var response = await _client.GetAsync("/employees?page=1&pageSize=-1");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(JsonOptions);
        body!.Status.Should().Be(400);
    }

    [Fact]
    public async Task UpdateEmployee_NotFound_Returns404()
    {
        var updateRequest = new UpdateEmployeeRequest(
            "Maria Santos", "maria.santos@email.com", "11912345678",
            "Av. Paulista", "1000", "Ap 1", "Bela Vista", "São Paulo", "SP", "01310100",
            EmployeeRole.Stockist);

        var response = await _client.PutAsJsonAsync($"/employees/{Guid.NewGuid()}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEmployee_NotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/employees/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostEmployee_DuplicateDocument_Returns409()
    {
        var document = "333.444.555-08";
        await CreateEmployee(document);

        var response = await _client.PostAsJsonAsync("/employees", ValidRequest(document));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<EmployeeResponse> CreateEmployee(string document)
    {
        var response = await _client.PostAsJsonAsync("/employees", ValidRequest(document));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions))!;
    }
}
