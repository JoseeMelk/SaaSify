using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SaaSify.Application.Interfaces;

namespace SaaSify.Infrastructure.Services;

/// <summary>
/// Implementación de IJwtTokenService usando System.IdentityModel.Tokens.Jwt.
/// 
/// JWT (JSON Web Token) tiene tres partes:
/// - Header: algoritmo de firma (HS256)
/// - Payload: claims (userId, email, exp)
/// - Signature: hash del header + payload con la clave secreta
/// 
/// La clave secreta NUNCA viaja en el token.
/// La firma garantiza que el token no fue modificado.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    // Claims son los datos que se guardan dentro del token.
    // Se usan constantes para evitar typos al leer/escribir.
    private const string UserIdClaim = "userId";
    private const string EmailClaim = "email";

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Se genera un JWT access token con duración corta (15 min por defecto).
    /// 
    /// El token contiene:
    /// - userId: para identificar al usuario sin ir a la BD
    /// - email: para mostrar en la UI sin ir a la BD
    /// - exp: fecha de expiración
    /// - jti: ID único del token (para blacklist futura)
    /// </summary>
    public string GenerateAccessToken(Guid userId, string email, int expiresIn = 15)
    {
        var secretKey = GetSecretKey();
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256);

        // Se definen los claims que irán dentro del token.
        var claims = new[]
        {
            new Claim(UserIdClaim, userId.ToString()),
            new Claim(EmailClaim, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único del token
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64) // fecha de emisión
        };

        // Se construye el token con todos los parámetros.
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: signingCredentials);

        // Se serializa el token a string (el formato: header.payload.signature)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Se genera un refresh token con duración larga (30 días por defecto).
    /// 
    /// Es un JWT separado — tiene diferente tiempo de vida y diferente propósito.
    /// Se usa SOLO para obtener nuevos access tokens.
    /// </summary>
    public string GenerateRefreshToken(Guid userId, int expiresInDays = 30)
    {
        var secretKey = GetSecretKey();
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256);

        // El refresh token solo incluye el userId — no necesita más datos.
        var claims = new[]
        {
            new Claim(UserIdClaim, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Se marca como refresh token para distinguirlo del access token.
            new Claim("token_type", "refresh")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expiresInDays),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Se valida un JWT y se extraen los claims si es válido.
    /// Devuelve null si el token es inválido o expirado.
    /// </summary>
    public IDictionary<string, string>? ValidateAccessToken(string token)
    {
        var secretKey = GetSecretKey();
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            // Se valida el token con los mismos parámetros con los que se generó.
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true, // se rechaza si ya expiró
                ClockSkew = TimeSpan.Zero // sin tolerancia de tiempo
            }, out SecurityToken validatedToken);

            // Se extraen los claims del token validado.
            var jwtToken = (JwtSecurityToken)validatedToken;
            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }
        catch
        {
            // Si la validación falla por cualquier razón, se devuelve null.
            return null;
        }
    }

    /// <summary>
    /// Se extrae el userId de un JWT válido.
    /// Devuelve null si el token es inválido.
    /// </summary>
    public Guid? GetUserIdFromToken(string token)
    {
        var claims = ValidateAccessToken(token);

        if (claims == null || !claims.TryGetValue(UserIdClaim, out var userIdStr))
            return null;

        if (Guid.TryParse(userIdStr, out var userId))
            return userId;

        return null;
    }

    /// <summary>
    /// Se obtiene la clave secreta desde la configuración.
    /// La clave debe tener mínimo 32 caracteres (256 bits) para HS256.
    /// </summary>
    private byte[] GetSecretKey()
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret no está configurado en appsettings.");

        if (secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret debe tener al menos 32 caracteres.");

        return Encoding.UTF8.GetBytes(secret);
    }
}