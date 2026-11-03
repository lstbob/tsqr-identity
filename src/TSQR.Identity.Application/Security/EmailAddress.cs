using System.Net.Mail;

namespace TSQR.Identity.Application.Security;

public static class EmailAddress
{
    public static bool TryNormalize(string? email, out string normalized)
    {
        normalized = string.Empty;
        var trimmed = email?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 || trimmed.Length > 200)
            return false;
        if (!MailAddress.TryCreate(trimmed, out _))
            return false;

        normalized = trimmed.ToLowerInvariant();
        return true;
    }
}
