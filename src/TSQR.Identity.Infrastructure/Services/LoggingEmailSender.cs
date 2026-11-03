using Microsoft.Extensions.Logging;
using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Infrastructure.Options;

namespace TSQR.Identity.Infrastructure.Services;

/// <summary>
/// Development-only delivery adapter. The raw token is written to the log only
/// when <see cref="EmailOptions.LogConfirmationToken"/> is explicitly enabled.
/// </summary>
public sealed class LoggingEmailSender(
    EmailOptions options,
    ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendEmailChangeConfirmationAsync(
        string newEmail,
        string token,
        DateTime expiresAt,
        CancellationToken ct = default)
    {
        if (!options.LogConfirmationToken)
        {
            logger.LogError(
                "Email delivery is not configured. Set Email:SmtpHost or enable " +
                "Email:LogConfirmationToken for local development.");
            throw new InvalidOperationException("Email delivery is not configured.");
        }

        logger.LogWarning(
            "Email change confirmation for {Email}. Expires at {ExpiresAt:O}. " +
            "Confirmation URL: {ConfirmationUrl}",
            newEmail,
            expiresAt,
            BuildConfirmationUrl(newEmail, token));

        return Task.CompletedTask;
    }

    private string BuildConfirmationUrl(string newEmail, string token)
    {
        var baseUrl = options.ConfirmationUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
            return $"token={token}";

        var separator = baseUrl.Contains('?') ? '&' : '?';
        return $"{baseUrl}{separator}email={Uri.EscapeDataString(newEmail)}&token={Uri.EscapeDataString(token)}";
    }
}
