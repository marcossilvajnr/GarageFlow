using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using AppSeparationOrderStatus = GarageFlow.Application.Stock.Enums.SeparationOrderStatus;
using AppStockItemType = GarageFlow.Application.Stock.Enums.StockItemType;
using AppStockOperationType = GarageFlow.Application.Stock.Enums.StockOperationType;
using AppSupplyUnit = GarageFlow.Application.Stock.Enums.SupplyUnit;
using AppCustomerDocumentType = GarageFlow.Application.Customers.Enums.CustomerDocumentType;
using AppEmployeeRole = GarageFlow.Application.Employees.Enums.EmployeeRole;
using GarageFlow.Api.Employees.DTOs;
using GarageFlow.Api.Executions.DTOs;
using GarageFlow.Api.Parts.DTOs;
using GarageFlow.Api.Stock.DTOs;
using GarageFlow.Domain.Stock;
using GarageFlow.Tests.Integration;
using AppExecutionOrderStatus = GarageFlow.Application.Executions.Enums.ExecutionOrderStatus;

namespace GarageFlow.Tests.Integration.Stock;

public sealed class SeparationExecutionIntegrationEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static int _employeeSeed;
    private static int _cpfSeed = 800_000_000;

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

    private async Task<Guid> CreateEmployee(AppEmployeeRole role)
    {
        var seed = Interlocked.Increment(ref _employeeSeed);
        var response = await _client.PostAsJsonAsync(
            "/employees",
            new CreateEmployeeRequest(
                $"Employee SepExec {seed}",
                AppCustomerDocumentType.Cpf,
                GenerateValidCpf(),
                $"sep-exec-{seed}@garageflow.test",
                $"1197{seed % 1_0000:D4}001",
                "Rua Integracao",
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

    private async Task<ExecutionOrderResponse> CreateExecutionOrder()
    {
        var response = await _client.PostAsJsonAsync(
            "/execution-orders",
            new CreateExecutionOrderRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions))!;
    }

    private async Task<Guid> CreatePart()
    {
        var request = new CreatePartRequest("Filtro de óleo", $"P-{Guid.NewGuid():N}"[..10], $"SKU-{Guid.NewGuid():N}"[..12], "UN", 50m);
        var response = await _client.PostAsJsonAsync("/parts", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PartResponse>(JsonOptions);
        return body!.Id;
    }

    private async Task<SeparationOrderResponse> CreateSeparatedSeparationOrder(Guid executionOrderId)
    {
        var stockistId = await CreateEmployee(AppEmployeeRole.Stockist);
        var partId = await CreatePart();
        var request = new CreateSeparationOrderRequest(
            executionOrderId,
            [new CreateSeparationPartItemRequest(partId, "Filtro de óleo", 1)],
            []);

        await SeedStockForRequest(request);

        var createResponse = await _client.PostAsJsonAsync(
            "/separation-orders",
            request);
        createResponse.EnsureSuccessStatusCode();
        var separation = (await createResponse.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;

        await _client.PostAsync($"/separation-orders/{separation.Id}/reserve", null);
        await _client.PostAsJsonAsync(
            $"/separation-orders/{separation.Id}/confirm-stockist-withdrawal",
            new ConfirmSeparationStockistWithdrawalRequest(stockistId));

        var separatedResponse = await _client.GetAsync($"/separation-orders/{separation.Id}");
        separatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var separated = (await separatedResponse.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;
        separated.Status.Should().Be(AppSeparationOrderStatus.Separated);

        return separated;
    }

    private async Task SeedStockForRequest(CreateSeparationOrderRequest request, decimal initialQuantity = 100m)
    {
        foreach (var part in request.Parts ?? [])
        {
            var response = await _client.PostAsJsonAsync(
                "/stock/entries",
                new CreateStockEntryRequest(part.PartId, AppStockItemType.Part, initialQuantity, 0m, "Seed integração separação", null));
            response.EnsureSuccessStatusCode();
        }

        foreach (var supply in request.Supplies ?? [])
        {
            var response = await _client.PostAsJsonAsync(
                "/stock/entries",
                new CreateStockEntryRequest(supply.SupplyId, AppStockItemType.Supply, initialQuantity, 0m, "Seed integração separação", null));
            response.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task ConfirmMechanicReceipt_MarksExecutionAsReady()
    {
        var execution = await CreateExecutionOrder();
        execution.Status.Should().Be(AppExecutionOrderStatus.Pending);

        var separation = await CreateSeparatedSeparationOrder(execution.Id);

        var confirmResponse = await _client.PostAsync(
            $"/separation-orders/{separation.Id}/confirm-mechanic-receipt",
            null);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var executionResponse = await _client.GetAsync($"/execution-orders/{execution.Id}");
        executionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedExecution = await executionResponse.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);

        updatedExecution!.Status.Should().Be(AppExecutionOrderStatus.Ready);
    }

    [Fact]
    public async Task ConfirmMechanicReceipt_WhenExecutionMissing_Returns404()
    {
        var separation = await CreateSeparatedSeparationOrder(Guid.NewGuid());

        var confirmResponse = await _client.PostAsync(
            $"/separation-orders/{separation.Id}/confirm-mechanic-receipt",
            null);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
