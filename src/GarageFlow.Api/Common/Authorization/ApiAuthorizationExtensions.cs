using Microsoft.AspNetCore.Authorization;

namespace GarageFlow.Api.Common.Authorization;

public static class ApiAuthorizationExtensions
{
    public static TBuilder RequireRoles<TBuilder>(this TBuilder builder, params string[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(policy =>
            policy.RequireAuthenticatedUser()
                .RequireRole(roles));
    }
}
