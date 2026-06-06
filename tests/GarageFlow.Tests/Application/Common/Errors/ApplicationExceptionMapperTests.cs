using FluentAssertions;
using GarageFlow.Application.Common.Errors;
using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Tests.Application.Common.Errors;

public sealed class ApplicationExceptionMapperTests
{
    [Theory]
    [MemberData(nameof(ExceptionMappings))]
    public void Map_WithKnownException_ReturnsExpectedErrorKind(
        Exception exception,
        ApplicationErrorKind expectedKind)
    {
        var descriptor = ApplicationExceptionMapper.Map(exception);

        descriptor.Kind.Should().Be(expectedKind);
    }

    [Fact]
    public void Map_WithUnknownException_ReturnsUnexpected()
    {
        var descriptor = ApplicationExceptionMapper.Map(new InvalidOperationException("boom"));

        descriptor.Kind.Should().Be(ApplicationErrorKind.Unexpected);
    }

    public static TheoryData<Exception, ApplicationErrorKind> ExceptionMappings() => new()
    {
        { new InvalidCredentialsException("invalid"), ApplicationErrorKind.Unauthorized },
        { new EntityNotFoundException("missing"), ApplicationErrorKind.NotFound },
        { new QuoteNotFoundException("missing quote"), ApplicationErrorKind.NotFound },
        { new DuplicateDocumentException("duplicate"), ApplicationErrorKind.Conflict },
        { new DuplicateStockDataException("duplicate stock"), ApplicationErrorKind.Conflict },
        { new InvalidServiceOrderStatusTransitionException("invalid state"), ApplicationErrorKind.StateConflict },
        { new InvalidSeparationOrderStatusTransitionException("invalid state"), ApplicationErrorKind.StateConflict },
        { new StockQuantityConflictException("stock conflict"), ApplicationErrorKind.Conflict },
        { new DiagnosticNoServicesException("no services"), ApplicationErrorKind.Conflict },
        { new InvalidLoginPayloadException("invalid login"), ApplicationErrorKind.Validation },
        { new SeparationOrderCustodyPreconditionException("custody"), ApplicationErrorKind.Validation },
        { new DomainException("domain"), ApplicationErrorKind.Validation }
    };
}
