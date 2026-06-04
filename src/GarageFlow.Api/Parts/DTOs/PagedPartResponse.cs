namespace GarageFlow.Api.Parts.DTOs;

public sealed record PagedPartResponse(
    IReadOnlyList<PartResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
