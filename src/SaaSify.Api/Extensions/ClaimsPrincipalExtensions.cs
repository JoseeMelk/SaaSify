using System.Security.Claims;

namespace SaaSify.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("userId")?.Value;

        return Guid.TryParse(value, out var id)
            ? id
            : Guid.Empty;
    }
}