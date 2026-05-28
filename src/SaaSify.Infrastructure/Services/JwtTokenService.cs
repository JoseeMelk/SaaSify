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
/// Dos tipos de tokens:
/// - Access token: vida corta (15 min), token_type = "access"
/// - Refresh token: vida larga (30 días), token_type = "refresh"
/// 
/// El claim token_type permite distinguirlos y rechazar
/// refresh tokens en endpoints que solo aceptan access tokens.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    private const string UserIdClaim = "userId";
    private const string EmailClaim = "email";
    private const string TokenTypeClaim = "token_type";
    private const string AccessTokenType = "access";
    private const string RefreshTokenType = "refresh";

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Se genera un JWT access token con token_type = "access".
    /// Solo este tipo de token es aceptado en endpoints protegidos.
    /// </summary>
    public string GenerateAccessToken(Guid userId, string email, int expiresIn = 15)
    {
        var secretKey = GetSecretKey();
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(UserIdClaim, userId.ToString()),
            new Claim(EmailClaim, email),
            new Claim(TokenTypeClaim, AccessTokenType), // ← distingue access de refresh
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Se genera un JWT refresh token con token_type = "refresh".
    /// Solo se acepta en el endpoint /api/auth/refresh — en ningún otro.
    /// </summary>
    public string GenerateRefreshToken(Guid userId, int expiresInDays = 30)
    {
        var secretKey = GetSecretKey();
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(UserIdClaim, userId.ToString()),
            new Claim(TokenTypeClaim, RefreshTokenType), // ← marca como refresh
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expiresInDays),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public IDictionary<string, string>? ValidateAccessToken(string token)
    {
        var secretKey = GetSecretKey();
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }
        catch
        {
            return null;
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        var claims = ValidateAccessToken(token);

        if (claims == null || !claims.TryGetValue(UserIdClaim, out var userIdStr))
            return null;

        if (Guid.TryParse(userIdStr, out var userId))
            return userId;

        return null;
    }

    private byte[] GetSecretKey()
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret no está configurado en appsettings.");

        if (secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret debe tener al menos 32 caracteres.");

        return Encoding.UTF8.GetBytes(secret);
    }
}