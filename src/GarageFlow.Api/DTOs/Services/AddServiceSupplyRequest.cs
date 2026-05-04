namespace GarageFlow.Api.DTOs.Services;

public sealed record AddServiceSupplyRequest(
    Guid SupplyId,
    decimal Quantity);
