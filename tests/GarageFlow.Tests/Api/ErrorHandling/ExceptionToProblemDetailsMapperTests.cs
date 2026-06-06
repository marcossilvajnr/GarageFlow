using FluentAssertions;
using GarageFlow.Api.Common.ErrorHandling;
using GarageFlow.Application.Common.Errors;
using Microsoft.AspNetCore.Http;

namespace GarageFlow.Tests.Api.ErrorHandling;

public sealed class ExceptionToProblemDetailsMapperTests
{
    [Theory]
    [MemberData(nameof(ErrorMappings))]
    public void Map_WithApplicationErrorKind_ReturnsExpectedStatusAndTitle(
        ApplicationErrorKind kind,
        int expectedStatusCode,
        string expectedTitle)
    {
        var mapping = ExceptionToProblemDetailsMapper.Map(new ApplicationErrorDescriptor(kind));

        mapping.StatusCode.Should().Be(expectedStatusCode);
        mapping.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public void Map_WithUnexpectedError_ReturnsInternalServerError()
    {
        var mapping = ExceptionToProblemDetailsMapper.Map(new ApplicationErrorDescriptor(ApplicationErrorKind.Unexpected));

        mapping.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        mapping.Title.Should().Be("Erro interno");
    }

    public static TheoryData<ApplicationErrorKind, int, string> ErrorMappings() => new()
    {
        { ApplicationErrorKind.Unauthorized, StatusCodes.Status401Unauthorized, "Não autorizado" },
        { ApplicationErrorKind.NotFound, StatusCodes.Status404NotFound, "Não encontrado" },
        { ApplicationErrorKind.Conflict, StatusCodes.Status409Conflict, "Conflito" },
        { ApplicationErrorKind.StateConflict, StatusCodes.Status409Conflict, "Conflito de estado" },
        { ApplicationErrorKind.Validation, StatusCodes.Status400BadRequest, "Erro de validação" }
    };
}
