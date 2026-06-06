using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Common.Pagination;

public static class ApiPagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    private const string InvalidPaginationDetail =
        "'page' deve ser maior ou igual a 1 e 'pageSize' deve estar entre 1 e 100.";

    public static bool IsValidPage(int page) =>
        page >= DefaultPage;

    public static bool IsValidPageSize(int pageSize) =>
        pageSize is >= 1 and <= MaxPageSize;

    public static bool IsValid(int page, int pageSize) =>
        IsValidPage(page) && IsValidPageSize(pageSize);

    public static ProblemDetails CreateInvalidPaginationProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Erro de validação",
        Detail = InvalidPaginationDetail
    };
}
