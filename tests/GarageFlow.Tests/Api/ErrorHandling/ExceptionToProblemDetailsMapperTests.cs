using FluentAssertions;
using GarageFlow.Api.ErrorHandling;
using GarageFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace GarageFlow.Tests.Api.ErrorHandling;

public sealed class ExceptionToProblemDetailsMapperTests
{
    [Theory]
    [MemberData(nameof(ExceptionMappings))]
    public void Map_WithKnownException_ReturnsExpectedStatusAndTitle(
        Exception exception,
        int expectedStatusCode,
        string expectedTitle)
    {
        var mapping = ExceptionToProblemDetailsMapper.Map(exception);

        mapping.StatusCode.Should().Be(expectedStatusCode);
        mapping.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public void Map_WithUnknownException_ReturnsInternalServerError()
    {
        var mapping = ExceptionToProblemDetailsMapper.Map(new InvalidOperationException("boom"));

        mapping.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        mapping.Title.Should().Be("Erro interno");
    }

    public static TheoryData<Exception, int, string> ExceptionMappings() => new()
    {
        { new InvalidCredentialsException("invalid"), StatusCodes.Status401Unauthorized, "Não autorizado" },
        { new EntityNotFoundException("missing"), StatusCodes.Status404NotFound, "Não encontrado" },
        { new QuoteNotFoundException("missing quote"), StatusCodes.Status404NotFound, "Não encontrado" },
        { new DuplicateDocumentException("duplicate"), StatusCodes.Status409Conflict, "Conflito" },
        { new DuplicateStockDataException("duplicate stock"), StatusCodes.Status409Conflict, "Conflito" },
        { new InvalidServiceOrderStatusTransitionException("invalid state"), StatusCodes.Status409Conflict, "Conflito de estado" },
        { new InvalidSeparationOrderStatusTransitionException("invalid state"), StatusCodes.Status409Conflict, "Conflito de estado" },
        { new StockQuantityConflictException("stock conflict"), StatusCodes.Status409Conflict, "Conflito" },
        { new DiagnosticNoServicesException("no services"), StatusCodes.Status409Conflict, "Conflito" },
        { new InvalidLoginPayloadException("invalid login"), StatusCodes.Status400BadRequest, "Erro de validação" },
        { new SeparationOrderCustodyPreconditionException("custody"), StatusCodes.Status400BadRequest, "Erro de validação" },
        { new DomainException("domain"), StatusCodes.Status400BadRequest, "Erro de validação" }
    };
}
