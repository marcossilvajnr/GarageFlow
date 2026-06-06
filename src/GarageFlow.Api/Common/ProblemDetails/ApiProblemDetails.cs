namespace GarageFlow.Api.Common.ProblemDetails;

public static class ApiProblemDetails
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateValidationProblemDetails(string detail) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Erro de validação",
        Detail = detail
    };
}
