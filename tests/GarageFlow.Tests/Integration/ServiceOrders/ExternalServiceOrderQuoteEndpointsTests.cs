using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.Customers.DTOs;
using GarageFlow.Api.Employees.DTOs;
using GarageFlow.Api.ServiceOrders.DTOs;
using GarageFlow.Api.Services.DTOs;
using GarageFlow.Api.Vehicles.DTOs;
using GarageFlow.Api.Common.Authorization;
using GarageFlow.Tests.Integration;
using AppCustomerDocumentType = GarageFlow.Application.Customers.Enums.CustomerDocumentType;
using AppEmployeeRole = GarageFlow.Application.Employees.Enums.EmployeeRole;
using AppQuoteStatus = GarageFlow.Application.ServiceOrders.Enums.QuoteStatus;

namespace GarageFlow.Tests.Integration.ServiceOrders;

public sealed class ExternalServiceOrderQuoteEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static int _cpfSeed = 200_000_000;
    private static int _renavamSeed = 2_000_000_000;
    private static int _licensePlateSeed = 500_000;
    private static int _serviceSeed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private HttpClient CreateClientWithRole(string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private static StringContent ExternalDecisionContent(
        Guid serviceOrderId,
        string decision,
        string? reason = null,
        string externalNotificationId = "ext",
        string source = "provider-x")
    {
        var reasonProperty = reason is null
            ? string.Empty
            : $"""
              ,
                "reason": "{reason}"
              """;

        return Json($$"""
        {
          "serviceOrderId": "{{serviceOrderId}}",
          "decision": "{{decision}}",
          "externalNotificationId": "{{externalNotificationId}}",
          "source": "{{source}}"{{reasonProperty}}
        }
        """);
    }

    private static StringContent Json(string json) =>
        new(json, Encoding.UTF8, "application/json");

    // ── HTTP matrix (task-055 §6.2) ──────────────────────────────────────────

    [Fact]
    public async Task PostExternalDecision_WithApproval_Returns200WithApprovedStatus()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(so.Id, "Approved", externalNotificationId: "ext-1"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quote = await response.Content.ReadFromJsonAsync<QuoteResponse>(JsonOptions);
        quote!.Status.Should().Be(AppQuoteStatus.CustomerApproved);
        quote.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PostExternalDecision_WithRejectionAndReason_Returns200WithRejectedStatus()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(
                so.Id,
                "Rejected",
                "Valor acima do esperado",
                externalNotificationId: "ext-2"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quote = await response.Content.ReadFromJsonAsync<QuoteResponse>(JsonOptions);
        quote!.Status.Should().Be(AppQuoteStatus.CustomerRejected);
        quote.RejectionReason.Should().Be("Valor acima do esperado");
    }

    [Fact]
    public async Task PostExternalDecision_WithoutDecision_Returns400()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            Json($$"""
            {
              "serviceOrderId": "{{so.Id}}",
              "externalNotificationId": "ext-3",
              "source": "provider-x"
            }
            """));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExternalDecision_WithNumericDecision_Returns400()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            Json($$"""
            {
              "serviceOrderId": "{{so.Id}}",
              "decision": 0,
              "externalNotificationId": "ext-4",
              "source": "provider-x"
            }
            """));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExternalDecision_WithLowercaseDecision_Returns400()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(so.Id, "approved", externalNotificationId: "ext-5"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExternalDecision_WithInvalidDecisionValue_Returns400()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(so.Id, "Invalid", externalNotificationId: "ext-6"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExternalDecision_WithEmptyServiceOrderId_Returns400()
    {
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(Guid.Empty, "Approved", externalNotificationId: "ext-7"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExternalDecision_WithEmptySource_Returns400()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(so.Id, "Approved", externalNotificationId: "ext-8", source: string.Empty));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExternalDecision_WithRejectionWithoutReason_Returns400()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(so.Id, "Rejected", externalNotificationId: "ext-9"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExternalDecision_WithNonExistentServiceOrder_Returns404()
    {
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(Guid.NewGuid(), "Approved", externalNotificationId: "ext-10"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostExternalDecision_WithNonExistentQuote_Returns404()
    {
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var so = await CreateServiceOrder(customer.Id, vehicle.Id);
        var externalClient = CreateClientWithRole(ApiRoles.External);

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(so.Id, "Approved", externalNotificationId: "ext-11"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostExternalDecision_WhenAlreadyDecided_Returns409()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var externalClient = CreateClientWithRole(ApiRoles.External);
        await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(so.Id, "Approved", externalNotificationId: "ext-12"));

        var response = await externalClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(so.Id, "Approved", externalNotificationId: "ext-13"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostExternalDecision_WithoutExternalRole_Returns403()
    {
        var so = await SetupOrderWithConsolidatedServices();
        await _client.PostAsync($"/service-orders/{so.Id}/quote/generate", null);
        var frontDeskClient = CreateClientWithRole(ApiRoles.FrontDesk);

        var response = await frontDeskClient.PostAsync(
            "/external/service-order-quote-notifications",
            ExternalDecisionContent(so.Id, "Approved", externalNotificationId: "ext-14"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Setup helpers (self-contained, mirrors ServiceOrdersEndpointsTests) ──

    private async Task<CustomerResponse> CreateCustomer(string document)
    {
        var request = new CreateCustomerRequest(
            "João Silva", AppCustomerDocumentType.Cpf, document,
            "joao@email.com", "11987654321",
            "Rua das Flores", "100", null, "Centro", "São Paulo", "SP", "01310100");

        var response = await _client.PostAsJsonAsync("/customers", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions))!;
    }

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

    private async Task<VehicleResponse> CreateVehicle(Guid customerId, string licensePlate, string renavam)
    {
        var request = new CreateVehicleRequest(customerId, licensePlate, renavam, "Toyota", "Corolla", 2020, "Branco");
        var response = await _client.PostAsJsonAsync("/vehicles", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VehicleResponse>(JsonOptions))!;
    }

    private static string GenerateValidLicensePlate()
    {
        var seed = Interlocked.Increment(ref _licensePlateSeed);
        var lettersSeed = seed / 10_000;
        var numbers = seed % 10_000;

        var first = (char)('A' + (lettersSeed / (26 * 26)) % 26);
        var second = (char)('A' + (lettersSeed / 26) % 26);
        var third = (char)('A' + lettersSeed % 26);

        return $"{first}{second}{third}-{numbers:D4}";
    }

    private static string GenerateValidRenavam()
    {
        var baseDigits = Interlocked.Increment(ref _renavamSeed).ToString("D10");
        var checkDigit = CalculateRenavamCheckDigit(baseDigits);
        return $"{baseDigits}{checkDigit}";
    }

    private static int CalculateRenavamCheckDigit(string firstTenDigits)
    {
        var multipliers = new[] { 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;

        for (var i = 0; i < firstTenDigits.Length; i++)
        {
            sum += (firstTenDigits[i] - '0') * multipliers[i];
        }

        var remainder = sum % 11;
        return remainder == 10 ? 0 : remainder;
    }

    private async Task<Guid> CreateEmployee(AppEmployeeRole role)
    {
        var seed = Interlocked.Increment(ref _serviceSeed);
        var request = new CreateEmployeeRequest(
            $"Funcionario Ext {seed}",
            AppCustomerDocumentType.Cpf,
            GenerateValidCpf(),
            $"funcionario-ext-{seed}@garageflow.test",
            $"1198{seed % 1_0000:D4}321",
            "Rua dos Funcionarios",
            "10",
            null,
            "Centro",
            "Sao Paulo",
            "SP",
            "01310100",
            role);

        var response = await _client.PostAsJsonAsync("/employees", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(JsonOptions);
        return body!.Id;
    }

    private async Task<ServiceOrderResponse> CreateServiceOrder(Guid customerId, Guid vehicleId)
    {
        var frontDeskEmployeeId = await CreateEmployee(AppEmployeeRole.Attendant);
        var request = new CreateServiceOrderRequest(customerId, vehicleId, frontDeskEmployeeId);
        var response = await _client.PostAsJsonAsync("/service-orders", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions))!;
    }

    private async Task<ServiceResponse> CreateService(string code, string name)
    {
        var request = new CreateServiceRequest(code, name, null, 100m, 60);
        var response = await _client.PostAsJsonAsync("/services", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions))!;
    }

    private async Task<ServiceOrderResponse> StartDiagnostic(Guid serviceOrderId, Guid mechanicId)
    {
        var effectiveMechanicId = mechanicId == Guid.Empty
            ? await CreateEmployee(AppEmployeeRole.Mechanic)
            : mechanicId;
        var request = new StartDiagnosticRequest(effectiveMechanicId);
        var response = await _client.PostAsJsonAsync($"/service-orders/{serviceOrderId}/diagnostic/start", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions))!;
    }

    private async Task<ServiceOrderResponse> CompleteDiagnostic(Guid serviceOrderId, string description)
    {
        var response = await _client.PostAsJsonAsync(
            $"/service-orders/{serviceOrderId}/diagnostic/complete",
            new CompleteDiagnosticRequest(description));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions))!;
    }

    private async Task<ServiceOrderResponse> ConsolidateDiagnosticServices(Guid serviceOrderId)
    {
        var response = await _client.PostAsync(
            $"/service-orders/{serviceOrderId}/diagnostic/consolidate-services", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions))!;
    }

    private async Task<ServiceOrderResponse> SetupOrderWithConsolidatedServices()
    {
        var seed = Interlocked.Increment(ref _serviceSeed);
        var customer = await CreateCustomer(GenerateValidCpf());
        var vehicle = await CreateVehicle(customer.Id, GenerateValidLicensePlate(), GenerateValidRenavam());
        var so = await CreateServiceOrder(customer.Id, vehicle.Id);
        var service = await CreateService($"SVC-EXT-{seed:D4}", $"Serviço Externo {seed:D4}");
        await StartDiagnostic(so.Id, Guid.Empty);
        await _client.PostAsJsonAsync($"/service-orders/{so.Id}/diagnostic/services",
            new AddDiagnosticServiceRequest(service.Id));
        await CompleteDiagnostic(so.Id, "Diagnóstico concluído para webhook externo.");
        await ConsolidateDiagnosticServices(so.Id);
        return so;
    }
}
