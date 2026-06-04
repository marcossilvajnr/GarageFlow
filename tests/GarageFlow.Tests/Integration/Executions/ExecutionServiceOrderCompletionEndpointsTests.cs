using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using AppCustomerDocumentType = GarageFlow.Application.Customers.Enums.CustomerDocumentType;
using AppEmployeeRole = GarageFlow.Application.Employees.Enums.EmployeeRole;
using GarageFlow.Api.Employees.DTOs;
using GarageFlow.Api.Executions.DTOs;
using GarageFlow.Api.Parts.DTOs;
using GarageFlow.Api.ServiceOrders.DTOs;
using GarageFlow.Api.Stock.DTOs;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Stock;
using GarageFlow.Infrastructure.Persistence;
using GarageFlow.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Tests.Integration.Executions;

public sealed class ExecutionServiceOrderCompletionEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static int _employeeSeed;
    private static int _cpfSeed = 600_000_000;

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
        var request = new CreateEmployeeRequest(
            $"Employee Execution Completion {seed}",
            AppCustomerDocumentType.Cpf,
            GenerateValidCpf(),
            $"execution-completion-employee-{seed}@garageflow.test",
            $"1193{seed % 1_0000:D4}321",
            "Rua Execucao",
            "60",
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

    /// <summary>
    /// Seeds a ServiceOrder in InExecution status directly into the SQLite test database.
    /// This bypasses the full API flow since StartExecutionFlow is not yet a standalone endpoint.
    /// </summary>
    private async Task<Guid> SeedServiceOrderInExecution()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();

        var so = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        typeof(ServiceOrder)
            .GetProperty(nameof(ServiceOrder.Status))!
            .SetValue(so, ServiceOrderStatus.InExecution);

        db.ServiceOrders.Add(so);
        await db.SaveChangesAsync();

        return so.Id;
    }

    private async Task<ExecutionOrderResponse> CreateExecution(Guid serviceOrderId)
    {
        var request = new CreateExecutionOrderRequest(serviceOrderId, Guid.NewGuid(), Guid.NewGuid());
        var response = await _client.PostAsJsonAsync("/execution-orders", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions))!;
    }

    private async Task<ExecutionOrderResponse> AdvanceToInExecution(Guid executionOrderId)
    {
        await _client.PostAsync($"/execution-orders/{executionOrderId}/mark-ready", null);
        var response = await _client.PostAsync($"/execution-orders/{executionOrderId}/start", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions))!;
    }

    private async Task<Guid> CreatePartWithStock(decimal initialQuantity = 10m)
    {
        var code = $"PC-{Guid.NewGuid():N}"[..10];
        var sku = $"SK-{Guid.NewGuid():N}"[..12];
        var partResp = await _client.PostAsJsonAsync("/parts",
            new CreatePartRequest("Filtro de óleo", code, sku, "UN", 50m));
        partResp.EnsureSuccessStatusCode();
        var part = (await partResp.Content.ReadFromJsonAsync<PartResponse>(JsonOptions))!;

        var stockResp = await _client.PostAsJsonAsync("/stock/entries",
            new CreateStockEntryRequest(part.Id, StockItemType.Part, initialQuantity, 0m, "Seed completion", null));
        stockResp.EnsureSuccessStatusCode();

        return part.Id;
    }

    private async Task CreateAndReserveSeparation(Guid executionOrderId, Guid partId)
    {
        var createResp = await _client.PostAsJsonAsync("/separation-orders",
            new CreateSeparationOrderRequest(
                executionOrderId,
                [new CreateSeparationPartItemRequest(partId, "Filtro de óleo", 1)],
                []));
        createResp.EnsureSuccessStatusCode();
        var separation = (await createResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;

        var reserveResp = await _client.PostAsync($"/separation-orders/{separation.Id}/reserve", null);
        reserveResp.EnsureSuccessStatusCode();
        await MarkSeparationAsCompleted(separation.Id);
    }

    private async Task MarkSeparationAsCompleted(Guid separationOrderId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
        var separation = await db.SeparationOrders.FindAsync(separationOrderId);
        separation.Should().NotBeNull();
        typeof(SeparationOrder)
            .GetProperty(nameof(SeparationOrder.Status))!
            .SetValue(separation!, SeparationOrderStatus.Completed);
        await db.SaveChangesAsync();
    }

    // --- POST /execution-orders/{id}/complete com execuções pendentes mantém OS em andamento ---

    [Fact]
    public async Task CompleteExecution_WhenSiblingExecutionsPending_ServiceOrderRemainsInExecution()
    {
        var serviceOrderId = await SeedServiceOrderInExecution();

        var first = await CreateExecution(serviceOrderId);
        var second = await CreateExecution(serviceOrderId);

        await AdvanceToInExecution(first.Id);
        await AdvanceToInExecution(second.Id);
        var partId1 = await CreatePartWithStock();
        var partId2 = await CreatePartWithStock();
        await CreateAndReserveSeparation(first.Id, partId1);
        await CreateAndReserveSeparation(second.Id, partId2);

        // Complete only the first execution
        var completeResponse = await _client.PostAsync($"/execution-orders/{first.Id}/complete", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var soResponse = await _client.GetAsync($"/service-orders/{serviceOrderId}");
        soResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var so = await soResponse.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        so!.Status.Should().Be(ServiceOrderStatus.InExecution);
    }

    // --- POST /execution-orders/{id}/complete com última execução leva OS ao status final ---

    [Fact]
    public async Task CompleteExecution_WhenLastExecution_ServiceOrderBecomesFinished()
    {
        var serviceOrderId = await SeedServiceOrderInExecution();

        var execution = await CreateExecution(serviceOrderId);
        await AdvanceToInExecution(execution.Id);
        var partId = await CreatePartWithStock();
        await CreateAndReserveSeparation(execution.Id, partId);

        var completeResponse = await _client.PostAsync($"/execution-orders/{execution.Id}/complete", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var soResponse = await _client.GetAsync($"/service-orders/{serviceOrderId}");
        soResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var so = await soResponse.Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        so!.Status.Should().Be(ServiceOrderStatus.Finished);
    }

    [Fact]
    public async Task CompleteExecution_WhenAllSiblingsCompleted_LastOneFinishesServiceOrder()
    {
        var serviceOrderId = await SeedServiceOrderInExecution();

        var first = await CreateExecution(serviceOrderId);
        var second = await CreateExecution(serviceOrderId);

        await AdvanceToInExecution(first.Id);
        await AdvanceToInExecution(second.Id);
        var partId1 = await CreatePartWithStock();
        var partId2 = await CreatePartWithStock();
        await CreateAndReserveSeparation(first.Id, partId1);
        await CreateAndReserveSeparation(second.Id, partId2);

        // Complete first — SO stays InExecution
        await _client.PostAsync($"/execution-orders/{first.Id}/complete", null);
        var soAfterFirst = await (await _client.GetAsync($"/service-orders/{serviceOrderId}"))
            .Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        soAfterFirst!.Status.Should().Be(ServiceOrderStatus.InExecution);

        // Complete second — SO transitions to Finished
        var lastCompleteResponse = await _client.PostAsync($"/execution-orders/{second.Id}/complete", null);
        lastCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var soAfterLast = await (await _client.GetAsync($"/service-orders/{serviceOrderId}"))
            .Content.ReadFromJsonAsync<ServiceOrderResponse>(JsonOptions);
        soAfterLast!.Status.Should().Be(ServiceOrderStatus.Finished);
    }

    // --- Erros: 404 para execução inexistente ---

    [Fact]
    public async Task CompleteExecution_WhenExecutionNotFound_Returns404()
    {
        var response = await _client.PostAsync($"/execution-orders/{Guid.NewGuid()}/complete", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Erros: 409 para transição inválida ---

    [Fact]
    public async Task CompleteExecution_WhenNotInExecution_Returns409()
    {
        var serviceOrderId = await SeedServiceOrderInExecution();
        var request = new CreateExecutionOrderRequest(serviceOrderId, Guid.NewGuid(), Guid.NewGuid());
        var created = await (await _client.PostAsJsonAsync("/execution-orders", request))
            .Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        var partId = await CreatePartWithStock();
        await CreateAndReserveSeparation(created!.Id, partId);

        var response = await _client.PostAsync($"/execution-orders/{created!.Id}/complete", null);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
