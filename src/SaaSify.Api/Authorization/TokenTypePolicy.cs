using Microsoft.AspNetCore.Authorization;

namespace SaaSify.Api.Authorization;

/// <summary>
/// Nombre de la policy que requiere un access token.
/// Se usa como constante para evitar strings hardcodeados en los controllers.
/// </summary>
public static class Policies
{
    public const string RequireAccessToken = "RequireAccessToken";
}

/// <summary>
/// Requisito de autorización: el token debe ser de tipo "access".
/// 
/// Este requisito es evaluado por AccessTokenRequirementHandler.
/// Si el token es de tipo "refresh", la request es rechazada con 403.
/// </summary>
public class AccessTokenRequirement : IAuthorizationRequirement
{
    // No necesita propiedades — solo marca que se requiere access token.
}

/// <summary>
/// Handler que evalúa el AccessTokenRequirement.
/// 
/// Se verifica que el claim "token_type" del JWT sea "access".
/// Si es "refresh" o no existe, la autorización falla.
/// 
/// ASP.NET Core llama a este handler automáticamente cuando
/// un endpoint tiene [Authorize(Policy = "RequireAccessToken")].
/// </summary>
public class AccessTokenRequirementHandler
    : AuthorizationHandler<AccessTokenRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccessTokenRequirement requirement)
    {
        // Se busca el claim token_type en el JWT del usuario autenticado.
        var tokenType = context.User.FindFirst("token_type")?.Value;

        // Si el claim existe y es "access", la autorización es exitosa.
        if (tokenType == "access")
        {
            context.Succeed(requirement);
        }
        // Si es "refresh" o no existe, la autorización falla.
        // ASP.NET devuelve 403 Forbidden automáticamente.

        return Task.CompletedTask;
    }
}