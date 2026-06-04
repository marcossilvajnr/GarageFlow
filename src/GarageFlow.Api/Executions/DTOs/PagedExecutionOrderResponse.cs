namespace GarageFlow.Api.Executions.DTOs;

public sealed record PagedExecutionOrderResponse(
    IReadOnlyList<ExecutionOrderResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
