using System.Security.Claims;

namespace RideAway.API.Extensions;

/// <summary>
/// Reads the authenticated caller's identity from the JWT. Handlers never take a
/// user id from the request body, so all identity resolution funnels through here.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the caller's user id. The JWT handler maps the "sub" claim onto
    /// <see cref="ClaimTypes.NameIdentifier"/> by default (MapInboundClaims).
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">The caller has no usable identity.</exception>
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

        // Tolerate the un-mapped claim type so the check works regardless of
        // whether inbound claim mapping is enabled on the token handler.
        return principal.HasClaim(c =>
            (c.Type == ClaimTypes.Role || c.Type == "role") &&
            string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase));
    }
}
