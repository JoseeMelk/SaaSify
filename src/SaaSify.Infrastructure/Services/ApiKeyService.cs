using System.Security.Cryptography;
using System.Text;
using SaaSify.Application.Interfaces;

namespace SaaSify.Infrastructure.Services;

/// <summary>
/// Implementación de IApiKeyService.
/// 
/// Se usa SHA-256 para hashear y RandomNumberGenerator para generar
/// bytes criptográficamente seguros.
/// 
/// Formato de la key: sk_live_{32 bytes en hex}
/// Ejemplo: sk_live_a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2
/// </summary>
public class ApiKeyService : IApiKeyService
{
    // Prefijo que identifica el ambiente de la key.
    // sk_live_ para producción — en el futuro sk_test_ para testing.
    private const string Prefix = "sk_live_";

    // Cantidad de bytes aleatorios — 32 bytes = 256 bits de entropía.
    // Representados en hex = 64 caracteres.
    private const int KeyByteLength = 32;

    // Longitud del prefijo de búsqueda (ej: "sk_live_a1b2").
    // Lo suficientemente largo para ser único, lo suficientemente corto para ser eficiente.
    private const int PrefixLength = 16;

    /// <summary>
    /// Se genera una API key segura con tres componentes:
    /// - RawKey: la key completa para mostrar al developer
    /// - Hash: SHA-256 del RawKey para guardar en BD
    /// - Prefix: primeros caracteres para lookup rápido
    /// </summary>
    public ApiKeyResult Generate()
    {
        // Se generan 32 bytes aleatorios criptográficamente seguros.
        var randomBytes = RandomNumberGenerator.GetBytes(KeyByteLength);

        // Se convierte a hexadecimal — más legible que base64 para una API key.
        var randomHex = Convert.ToHexString(randomBytes).ToLowerInvariant();

        // Se construye la key completa con el prefijo.
        var rawKey = $"{Prefix}{randomHex}";

        // Se calcula el SHA-256 del rawKey para guardarlo en la BD.
        var hash = ComputeHash(rawKey);

        // Se extrae el prefijo de búsqueda — los primeros PrefixLength caracteres.
        // Permite buscar en la BD sin necesidad de hashear toda la key entrante.
        var prefix = rawKey[..PrefixLength];

        return new ApiKeyResult(rawKey, hash, prefix);
    }

    /// <summary>
    /// Se verifica que una raw key coincida con el hash guardado.
    /// Se usa cuando el developer envía su key en el header X-Api-Key.
    /// </summary>
    public bool Verify(string rawKey, string hash)
    {
        var computedHash = ComputeHash(rawKey);

        // Se usa CryptographicOperations.FixedTimeEquals para evitar
        // timing attacks — la comparación siempre tarda el mismo tiempo
        // independientemente de cuántos caracteres coincidan.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(hash));
    }

    /// <summary>
    /// Se calcula el SHA-256 de un string y se devuelve en hexadecimal.
    /// </summary>
    private static string ComputeHash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}