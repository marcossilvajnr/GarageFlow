namespace GarageFlow.Application.Executions.DTOs;

public sealed record PagedExecutionOrderResult(
    IReadOnlyList<ExecutionOrderDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
