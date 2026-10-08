using System.Security.Claims;

namespace RideAway.API.Extensions;

/// <summary>Caller identity from the JWT. Handlers never take ids from the body.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <exception cref="UnauthorizedAccessException">The token has no usable identity.</exception>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue("sub");

        if (!Guid.TryParse(raw, out var userId) || userId == Guid.Empty)
            throw new UnauthorizedAccessException("The authenticated token does not identify a user.");

        return userId;
    }

    public static bool IsInUserRole(this ClaimsPrincipal principal, string role)
    {
        if (principal.IsInRole(role))
            return true;

        // Works with or without inbound claim mapping on the token handler.
        return principal.HasClaim(c =>
            (c.Type == ClaimTypes.Role || c.Type == "role") &&
            string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase));
    }
}
