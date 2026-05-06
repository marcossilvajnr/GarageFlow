using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GarageFlow.Tests.Integration;

public sealed class TestAuthSchemeOptions : AuthenticationSchemeOptions;

public sealed class TestAuthHandler(
    IOptionsMonitor<TestAuthSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<TestAuthSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "TestAuth";
    internal const string RoleHeader = "X-Test-Role";
    internal const string SubHeader = "X-Test-Sub";
    internal const string ForceAnonymousHeader = "X-Test-Anonymous";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.TryGetValue(ForceAnonymousHeader, out var anonymousHeaderValue)
            && bool.TryParse(anonymousHeaderValue.ToString(), out var forceAnonymous)
            && forceAnonymous)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var role = Request.Headers.TryGetValue(RoleHeader, out var roleValues)
            ? roleValues.ToString()
            : "Administrative";

        var sub = Request.Headers.TryGetValue(SubHeader, out var subValues)
            ? subValues.ToString()
            : "test-user";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, sub),
            new Claim(ClaimTypes.Name, sub),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
