using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GarageFlow.Api.DTOs.Executions;
using GarageFlow.Api.DTOs.Stock;
using GarageFlow.Domain.Executions;
using GarageFlow.Domain.Stock;
using GarageFlow.Tests.Integration;

namespace GarageFlow.Tests.Integration.Stock;

public sealed class SeparationExecutionIntegrationEndpointsTests(GarageFlowWebApplicationFactory factory)
    : IClassFixture<GarageFlowWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<ExecutionOrderResponse> CreateExecutionOrder()
    {
        var response = await _client.PostAsJsonAsync(
            "/execution-orders",
            new CreateExecutionOrderRequest(Guid.NewGuid(), Guid.NewGuid()));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions))!;
    }

    private async Task<SeparationOrderResponse> CreateSeparatedSeparationOrder(Guid executionOrderId)
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/separation-orders",
            new CreateSeparationOrderRequest(
                executionOrderId,
                [new CreateSeparationPartItemRequest(Guid.NewGuid(), "Filtro de óleo", 1)],
                []));
        createResponse.EnsureSuccessStatusCode();
        var separation = (await createResponse.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;

        await _client.PostAsync($"/separation-orders/{separation.Id}/reserve", null);
        await _client.PostAsJsonAsync(
            $"/separation-orders/{separation.Id}/confirm-stockist-withdrawal",
            new ConfirmSeparationStockistWithdrawalRequest(Guid.NewGuid()));

        var separatedResponse = await _client.GetAsync($"/separation-orders/{separation.Id}");
        separatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var separated = (await separatedResponse.Content.ReadFromJsonAsync<SeparationOrderResponse>(JsonOptions))!;
        separated.Status.Should().Be(SeparationOrderStatus.Separated);

        return separated;
    }

    [Fact]
    public async Task ConfirmMechanicReceipt_MarksExecutionAsReady()
    {
        var execution = await CreateExecutionOrder();
        execution.Status.Should().Be(ExecutionOrderStatus.Pending);

        var separation = await CreateSeparatedSeparationOrder(execution.Id);

        var confirmResponse = await _client.PostAsync(
            $"/separation-orders/{separation.Id}/confirm-mechanic-receipt",
            null);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var executionResponse = await _client.GetAsync($"/execution-orders/{execution.Id}");
        executionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedExecution = await executionResponse.Content.ReadFromJsonAsync<ExecutionOrderResponse>(JsonOptions);

        updatedExecution!.Status.Should().Be(ExecutionOrderStatus.Ready);
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
