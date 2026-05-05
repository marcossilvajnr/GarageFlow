using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Executions;
using GarageFlow.Api.DTOs.Parts;
using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Api.DTOs.Supplies;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;
using GarageFlow.Infrastructure.Persistence;
using GarageFlow.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Tests.Integration.Executions;

public sealed class ExecutionStockConsumptionEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<Guid> CreatePartWithStock(decimal initialQuantity = 100m)
    {
        var code = $"PC-{Guid.NewGuid():N}"[..10];
        var sku = $"SK-{Guid.NewGuid():N}"[..12];
        var partResp = await _client.PostAsJsonAsync("/parts",
            new CreatePartRequest("Filtro de óleo", code, sku, "UN", 50m));
        partResp.EnsureSuccessStatusCode();
        var part = (await partResp.Content.ReadFromJsonAsync<PartResponse>(JsonOptions))!;

        var stockResp = await _client.PostAsJsonAsync("/stock/entries",
            new CreateStockEntryRequest(part.Id, StockItemType.Part, initialQuantity, 0m, "Seed consumo", null));
        stockResp.EnsureSuccessStatusCode();

        return part.Id;
    }

    private async Task<Guid> CreateSupplyWithStock(decimal initialQuantity = 100m)
    {
        var code = $"SC-{Guid.NewGuid():N}"[..10];
        var supplyResp = await _client.PostAsJsonAsync("/supplies",
            new CreateSupplyRequest("Óleo 5W30", code, "L", 25m, null));
        supplyResp.EnsureSuccessStatusCode();
        var supply = (await supplyResp.Content.ReadFromJsonAsync<SupplyResponse>(JsonOptions))!;

        var stockResp = await _client.PostAsJsonAsync("/stock/entries",
            new CreateStockEntryRequest(supply.Id, StockItemType.Supply, initialQuantity, 0m, "Seed consumo", null));
        stockResp.EnsureSuccessStatusCode();

        return supply.Id;
    }

    private async Task<SeparationOrderResponse> CreateAndReserveSeparation(Guid executionOrderId, Guid partId)
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

        return separation;
    }

    private async Task<SeparationOrderResponse> CreateAndReserveSupplySeparation(Guid executionOrderId, Guid supplyId)
    {
        var createResp = await _client.PostAsJsonAsync("/separation-orders",
            new CreateSeparationOrderRequest(
                executionOrderId,
                [],
                [new CreateSeparationSupplyItemRequest(supplyId, "Óleo 5W30", 2m, SupplyUnit.Liter)]));
        createResp.EnsureSuccessStatusCode();
        var separation = (await createResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;

        var reserveResp = await _client.PostAsync($"/separation-orders/{separation.Id}/reserve", null);
        reserveResp.EnsureSuccessStatusCode();
        await MarkSeparationAsCompleted(separation.Id);

        return separation;
    }

    private async Task MarkSeparationAsCompleted(Guid separationOrderId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
        var separation = await db.SeparationOrders.FindAsync(separationOrderId);
        separation.Should().NotBeNull();
        typeof(SeparationOrder).GetProperty(nameof(SeparationOrder.Status))!
            .SetValue(separation!, SeparationOrderStatus.Completed);
        await db.SaveChangesAsync();
    }

    private async Task<ExecutionOrderResponse> CreateAndStartExecution()
    {
        var serviceOrderId = await SeedServiceOrderInExecution();

        var createResp = await _client.PostAsJsonAsync("/execution-orders",
            new CreateExecutionOrderRequest(serviceOrderId, Guid.NewGuid()));
        createResp.EnsureSuccessStatusCode();
        var execution = (await createResp.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions))!;

        await _client.PostAsync($"/execution-orders/{execution.Id}/mark-ready", null);
        await _client.PostAsJsonAsync($"/execution-orders/{execution.Id}/start",
            new StartExecutionOrderRequest(Guid.NewGuid()));

        return execution;
    }

    private async Task<Guid> SeedServiceOrderInExecution()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
        var serviceOrder = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        typeof(ServiceOrder)
            .GetProperty(nameof(ServiceOrder.Status))!
            .SetValue(serviceOrder, ServiceOrderStatus.InExecution);
        db.ServiceOrders.Add(serviceOrder);
        await db.SaveChangesAsync();
        return serviceOrder.Id;
    }

    // --- Stock is NOT consumed on execution complete (baixa ocorre em ConfirmStockistWithdrawal) ---

    [Fact]
    public async Task CompleteExecution_WithSeparationCompleted_Returns200AndDoesNotAlterStockBalance()
    {
        var execution = await CreateAndStartExecution();
        var partId = await CreatePartWithStock(initialQuantity: 10m);
        await CreateAndReserveSeparation(execution.Id, partId);

        var stockBefore = (await (await _client.GetAsync($"/stock/Part/{partId}"))
            .Content.ReadFromJsonAsync<StockPositionResponse>(JsonOptions))!;

        var completeResp = await _client.PostAsync($"/execution-orders/{execution.Id}/complete", null);

        completeResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await completeResp.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);
        body!.Status.Should().Be(ExecutionOrderStatus.Completed);

        var stockAfter = (await (await _client.GetAsync($"/stock/Part/{partId}"))
            .Content.ReadFromJsonAsync<StockPositionResponse>(JsonOptions))!;

        // A baixa definitiva ocorre em ConfirmStockistWithdrawal, não em CompleteExecution.
        stockAfter.TotalQuantity.Should().Be(stockBefore.TotalQuantity);
        stockAfter.ReservedQuantity.Should().Be(stockBefore.ReservedQuantity);
        stockAfter.AvailableQuantity.Should().Be(stockBefore.AvailableQuantity);
    }

    [Fact]
    public async Task CompleteExecution_WhenSeparationNotFound_Returns404()
    {
        var execution = await CreateAndStartExecution();
        // Intentionally no separation order created

        var response = await _client.PostAsync($"/execution-orders/{execution.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteExecution_WithSeparationCompleted_Returns200RegardlessOfStockPresence()
    {
        var execution = await CreateAndStartExecution();

        // Create a separation for a partId that has no stock entry — CompleteExecution should succeed
        // because it no longer accesses stock; only ConfirmStockistWithdrawal does.
        var orphanPartId = Guid.NewGuid();
        var createSepResp = await _client.PostAsJsonAsync("/separation-orders",
            new CreateSeparationOrderRequest(
                execution.Id,
                [new CreateSeparationPartItemRequest(orphanPartId, "Peça sem estoque", 1)],
                []));
        createSepResp.EnsureSuccessStatusCode();
        var separation = (await createSepResp.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;
        await MarkSeparationAsCompleted(separation.Id);

        var response = await _client.PostAsync($"/execution-orders/{execution.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CompleteExecution_WhenStockReservedInsufficient_Returns409()
    {
        var execution = await CreateAndStartExecution();
        var partId = await CreatePartWithStock(initialQuantity: 10m);

        // Create separation but do NOT reserve stock (so ReservedQuantity stays 0)
        var createSepResp = await _client.PostAsJsonAsync("/separation-orders",
            new CreateSeparationOrderRequest(
                execution.Id,
                [new CreateSeparationPartItemRequest(partId, "Filtro de óleo", 1)],
                []));
        createSepResp.EnsureSuccessStatusCode();
        // Intentionally skip reserve call

        var response = await _client.PostAsync($"/execution-orders/{execution.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CompleteExecution_WithReservedSupplyStock_Returns200AndDoesNotAlterStockBalance()
    {
        var execution = await CreateAndStartExecution();
        var supplyId = await CreateSupplyWithStock(initialQuantity: 10m);
        await CreateAndReserveSupplySeparation(execution.Id, supplyId);

        var stockBefore = (await (await _client.GetAsync($"/stock/Supply/{supplyId}"))
            .Content.ReadFromJsonAsync<StockPositionResponse>(JsonOptions))!;

        var completeResp = await _client.PostAsync($"/execution-orders/{execution.Id}/complete", null);

        completeResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var stockAfter = (await (await _client.GetAsync($"/stock/Supply/{supplyId}"))
            .Content.ReadFromJsonAsync<StockPositionResponse>(JsonOptions))!;

        // A baixa definitiva ocorre em ConfirmStockistWithdrawal, não em CompleteExecution.
        stockAfter.TotalQuantity.Should().Be(stockBefore.TotalQuantity);
        stockAfter.ReservedQuantity.Should().Be(stockBefore.ReservedQuantity);
        stockAfter.AvailableQuantity.Should().Be(stockBefore.AvailableQuantity);
    }
}
