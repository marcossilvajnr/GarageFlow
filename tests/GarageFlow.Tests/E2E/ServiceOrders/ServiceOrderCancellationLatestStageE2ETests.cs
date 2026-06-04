using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using AppEmployeeRole = GarageFlow.Application.Employees.Enums.EmployeeRole;
using GarageFlow.Api.Customers.DTOs;
using GarageFlow.Api.Employees.DTOs;
using GarageFlow.Api.Executions.DTOs;
using GarageFlow.Api.Purchasing.DTOs;
using GarageFlow.Api.ServiceOrders.DTOs;
using GarageFlow.Api.Services.DTOs;
using GarageFlow.Api.Stock.DTOs;
using GarageFlow.Api.Vehicles.DTOs;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Tests.E2E.Infrastructure;

using AppCustomerDocumentType = GarageFlow.Application.Customers.Enums.CustomerDocumentType;

namespace GarageFlow.Tests.E2E.ServiceOrders;

[Collection("E2E Real DB")]
public sealed class ServiceOrderCancellationLatestStageE2ETests : E2ETestBase
{
    private readonly HttpClient _client;

    private static int _cpfSeed = 400_000_000;
    private static int _renavamSeed = 1_600_000_000;
    private static int _licensePlateSeed;
    private static int _employeeSeed;

    public ServiceOrderCancellationLatestStageE2ETests(E2ERealDbWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CustomerCancellation_AtLatestAllowedStage_ShouldRejectQuoteAndBlockForwardProgress()
    {
        await ResetRealDatabaseAsync(_client);
        await AuthenticateAsAsync(_client, E2ERole.Administrative);
        var frontDeskEmployeeId = await CreateEmployeeAsync(AppEmployeeRole.Attendant);
        var mechanicEmployeeId = await CreateEmployeeAsync(AppEmployeeRole.Mechanic);

        var customer = await CreateCustomerAsync();
        var vehicle = await CreateVehicleAsync(customer.Id);
        var service = await CreateServiceAsync();

        var createServiceOrderResponse = await _client.PostAsJsonAsync(
            "/service-orders",
            new CreateServiceOrderRequest(customer.Id, vehicle.Id, frontDeskEmployeeId));
        createServiceOrderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var serviceOrder = await ReadAsync<ServiceOrderResponse>(createServiceOrderResponse);
        serviceOrder.Status.Should().Be(ServiceOrderStatus.Received);

        var startDiagnosticResponse = await _client.PostAsJsonAsync(
            $"/service-orders/{serviceOrder.Id}/diagnostic/start",
            new StartDiagnosticRequest(mechanicEmployeeId));
        startDiagnosticResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var inDiagnosticOrder = await ReadAsync<ServiceOrderResponse>(startDiagnosticResponse);
        inDiagnosticOrder.Status.Should().Be(ServiceOrderStatus.InDiagnostic);

        var addDiagnosticServiceResponse = await _client.PostAsJsonAsync(
            $"/service-orders/{serviceOrder.Id}/diagnostic/services",
            new AddDiagnosticServiceRequest(service.Id));
        addDiagnosticServiceResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var completeDiagnosticResponse = await _client.PostAsJsonAsync(
            $"/service-orders/{serviceOrder.Id}/diagnostic/complete",
            new CompleteDiagnosticRequest("Cliente solicitou avaliação inicial e aguardará decisão."));
        completeDiagnosticResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var consolidateResponse = await _client.PostAsync(
            $"/service-orders/{serviceOrder.Id}/diagnostic/consolidate-services",
            null);
        consolidateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var generateQuoteResponse = await _client.PostAsync(
            $"/service-orders/{serviceOrder.Id}/quote/generate",
            null);
        generateQuoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var generatedQuote = await ReadAsync<QuoteResponse>(generateQuoteResponse);
        generatedQuote.Status.Should().Be(QuoteStatus.WaitingCustomerApproval);

        var waitingApprovalOrder = await GetServiceOrderAsync(serviceOrder.Id);
        waitingApprovalOrder.Status.Should().Be(ServiceOrderStatus.WaitingApproval);

        // Last allowed cancellation stage in current API/canonical implementation:
        // customer quote rejection while waiting approval.
        const string rejectionReason = "Cliente desistiu do serviço neste momento.";
        var rejectQuoteResponse = await _client.PostAsJsonAsync(
            $"/service-orders/{serviceOrder.Id}/quote/reject",
            new RejectQuoteRequest(rejectionReason));
        rejectQuoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var rejectedQuote = await ReadAsync<QuoteResponse>(rejectQuoteResponse);
        rejectedQuote.Status.Should().Be(QuoteStatus.CustomerRejected);
        rejectedQuote.RejectionReason.Should().Be(rejectionReason);
        rejectedQuote.RejectedAt.Should().NotBeNull();

        var rejectedServiceOrder = await GetServiceOrderAsync(serviceOrder.Id);
        rejectedServiceOrder.Status.Should().Be(ServiceOrderStatus.Rejected);

        // Forward progression must be blocked after cancellation.
        var acceptAfterRejectResponse = await _client.PostAsync(
            $"/service-orders/{serviceOrder.Id}/quote/accept",
            null);
        acceptAfterRejectResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var restartDiagnosticResponse = await _client.PostAsJsonAsync(
            $"/service-orders/{serviceOrder.Id}/diagnostic/start",
            new StartDiagnosticRequest(mechanicEmployeeId));
        restartDiagnosticResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var regenerateQuoteResponse = await _client.PostAsync(
            $"/service-orders/{serviceOrder.Id}/quote/generate",
            null);
        regenerateQuoteResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Correlated aggregates should not be created in cancellation flow.
        var executionListResponse = await _client.GetAsync("/execution-orders?page=1&pageSize=20");
        executionListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var executionList = await ReadAsync<PagedExecutionOrderResponse>(executionListResponse);
        executionList.TotalCount.Should().Be(0);

        var separationListResponse = await _client.GetAsync("/separation-orders?page=1&pageSize=20");
        separationListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var separationList = await ReadAsync<PagedSeparationOrderResponse>(separationListResponse);
        separationList.TotalCount.Should().Be(0);

        var purchaseListResponse = await _client.GetAsync("/purchase-orders?page=1&pageSize=20");
        purchaseListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var purchaseList = await ReadAsync<PagedPurchaseOrderResponse>(purchaseListResponse);
        purchaseList.TotalCount.Should().Be(0);
    }

    private async Task<CustomerResponse> CreateCustomerAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/customers",
            new CreateCustomerRequest(
                "Cliente E2E Cancelamento",
                AppCustomerDocumentType.Cpf,
                GenerateValidCpf(),
                $"cliente-cancelamento-{Guid.NewGuid():N}@garageflow.test",
                "11987654321",
                "Rua E2E",
                "100",
                null,
                "Centro",
                "São Paulo",
                "SP",
                "01310100"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<CustomerResponse>(response);
    }

    private async Task<VehicleResponse> CreateVehicleAsync(Guid customerId)
    {
        var response = await _client.PostAsJsonAsync(
            "/vehicles",
            new CreateVehicleRequest(
                customerId,
                GenerateValidLicensePlate(),
                GenerateValidRenavam(),
                "Toyota",
                "Corolla",
                2021,
                "Branco"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<VehicleResponse>(response);
    }

    private async Task<ServiceResponse> CreateServiceAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/services",
            new CreateServiceRequest(
                UniqueCode("SVC", 12),
                "Serviço E2E Cancelamento",
                "Fluxo de cancelamento no limite permitido",
                150m,
                60));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsync<ServiceResponse>(response);
    }

    private async Task<Guid> CreateEmployeeAsync(AppEmployeeRole role)
    {
        var seed = Interlocked.Increment(ref _employeeSeed);
        var response = await _client.PostAsJsonAsync(
            "/employees",
            new CreateEmployeeRequest(
                $"Funcionario E2E Cancelamento {seed}",
                AppCustomerDocumentType.Cpf,
                GenerateValidCpf(),
                $"funcionario-e2e-cancelamento-{seed}@garageflow.test",
                $"1190{seed % 1_0000:D4}321",
                "Rua E2E",
                "10",
                null,
                "Centro",
                "Sao Paulo",
                "SP",
                "01310100",
                role));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var employee = await ReadAsync<EmployeeResponse>(response);
        return employee.Id;
    }

    private async Task<ServiceOrderResponse> GetServiceOrderAsync(Guid serviceOrderId)
    {
        var response = await _client.GetAsync($"/service-orders/{serviceOrderId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await ReadAsync<ServiceOrderResponse>(response);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response) where T : class
        => (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;

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
}
