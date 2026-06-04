using System.Net.Http.Json;
using GarageFlow.Api.Customers.DTOs;
using GarageFlow.Api.Services.DTOs;
using GarageFlow.Api.Vehicles.DTOs;
using GarageFlow.Application.Customers.Enums;
using GarageFlow.Tests.E2E.Infrastructure;

namespace GarageFlow.Tests.E2E.Builders;

public sealed class E2ESeedBuilder(HttpClient client)
{
    private readonly HttpClient _client = client;

    public async Task<CustomerResponse> CreateCustomerAsync(string cpf)
    {
        var request = new CreateCustomerRequest(
            "João E2E",
            CustomerDocumentType.Cpf,
            cpf,
            "joao.e2e@garageflow.test",
            "11987654321",
            "Rua das Flores",
            "100",
            null,
            "Centro",
            "São Paulo",
            "SP",
            "01310100");

        var response = await _client.PostAsJsonAsync("/customers", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>(E2ETestBase.JsonOptions))!;
    }

    public async Task<VehicleResponse> CreateVehicleAsync(Guid customerId, string licensePlate, string renavam)
    {
        var request = new CreateVehicleRequest(
            customerId,
            licensePlate,
            renavam,
            "Toyota",
            "Corolla",
            2020,
            "Branco");

        var response = await _client.PostAsJsonAsync("/vehicles", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>(E2ETestBase.JsonOptions))!;
    }

    public async Task<ServiceResponse> CreateServiceAsync()
    {
        var request = new CreateServiceRequest(
            E2ETestBase.UniqueCode("SVC", 15),
            "Servico E2E",
            "Servico para smoke E2E",
            150m,
            60);

        var response = await _client.PostAsJsonAsync("/services", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceResponse>(E2ETestBase.JsonOptions))!;
    }
}
