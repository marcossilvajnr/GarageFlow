using GarageFlow.Application.Common.Errors;

namespace GarageFlow.Api.ErrorHandling;

public static class ExceptionToProblemDetailsMapper
{
    public static ExceptionMapping Map(ApplicationErrorDescriptor error) =>
        error.Kind switch
        {
            ApplicationErrorKind.Unauthorized =>
                new(StatusCodes.Status401Unauthorized, "Não autorizado"),

            ApplicationErrorKind.NotFound =>
                new(StatusCodes.Status404NotFound, "Não encontrado"),

            ApplicationErrorKind.Conflict =>
                new(StatusCodes.Status409Conflict, "Conflito"),

            ApplicationErrorKind.StateConflict =>
                new(StatusCodes.Status409Conflict, "Conflito de estado"),

            ApplicationErrorKind.Validation =>
                new(StatusCodes.Status400BadRequest, "Erro de validação"),

            _ => new(StatusCodes.Status500InternalServerError, "Erro interno")
        };
}
