namespace GarageFlow.Api.Services.DTOs;

public sealed record AddServiceSupplyRequest(
    Guid SupplyId,
    decimal Quantity);
