namespace GarageFlow.Api.DTOs.Executions;

public sealed record PagedExecutionOrderResponse(
    IReadOnlyList<ExecutionOrderResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
