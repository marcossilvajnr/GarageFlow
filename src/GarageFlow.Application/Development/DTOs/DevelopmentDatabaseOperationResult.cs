namespace GarageFlow.Application.Development.DTOs;

public sealed record DevelopmentDatabaseOperationResult(
    bool IsSuccess,
    string Message,
    string? Provider = null,
    string? ValidationDetail = null)
{
    public static DevelopmentDatabaseOperationResult Success(string message, string? provider = null)
        => new(true, message, provider);

    public static DevelopmentDatabaseOperationResult ValidationFailure(string detail)
        => new(false, string.Empty, ValidationDetail: detail);
}
