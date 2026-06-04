using GarageFlow.Application.Executions.DTOs;
using GarageFlow.Application.Executions.Mappers;
using GarageFlow.Domain.Executions;

namespace GarageFlow.Application.Executions.Handlers;

internal static class ExecutionOrderMapper
{
    internal static ExecutionOrderDto ToDto(ExecutionOrder executionOrder) =>
        new(
            executionOrder.Id,
            executionOrder.ServiceOrderId,
            executionOrder.ServiceId,
            executionOrder.MechanicId,
            ExecutionOrderStatusMapper.ToApplication(executionOrder.Status),
            executionOrder.StartedAt,
            executionOrder.CompletedAt,
            executionOrder.ActualTimeMinutes,
            executionOrder.CreatedAt);
}
