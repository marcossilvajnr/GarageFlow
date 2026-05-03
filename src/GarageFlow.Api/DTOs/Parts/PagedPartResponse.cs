namespace GarageFlow.Api.DTOs.Parts;

public sealed record PagedPartResponse(
    IReadOnlyList<PartResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
