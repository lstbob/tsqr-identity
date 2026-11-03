using System.Security.Cryptography;
using System.Text;

namespace TSQR.Identity.Application.Security;

/// <summary>
/// Generates URL-safe confirmation tokens and the deterministic SHA-256 hash
/// that is persisted in <c>pending_email_changes.TokenHash</c>.
/// </summary>
public static class EmailChangeToken
{
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string Hash(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token must be provided.", nameof(token));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim())));
    }
}
