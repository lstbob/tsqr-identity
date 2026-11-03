using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Infrastructure.Options;

namespace TSQR.Identity.Infrastructure.Services;

/// <summary>
/// SMTP delivery adapter used when <see cref="EmailOptions.SmtpHost"/> is
/// configured. The raw token is contained only in the email body.
/// </summary>
public sealed class SmtpEmailSender(
    EmailOptions options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendEmailChangeConfirmationAsync(
        string newEmail,
        string token,
        DateTime expiresAt,
        CancellationToken ct = default)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(options.FromAddress, options.FromName),
            Subject = "Confirm your new TownsSquare email address",
            Body = BuildBody(newEmail, token, expiresAt),
            IsBodyHtml = false,
        };
        message.To.Add(newEmail);

        using var client = new SmtpClient(options.SmtpHost, options.SmtpPort)
        {
            EnableSsl = options.SmtpUseSsl,
        };

        if (!string.IsNullOrWhiteSpace(options.SmtpUserName))
        {
            client.Credentials = new NetworkCredential(
                options.SmtpUserName, options.SmtpPassword);
        }

        try
        {
            await client.SendMailAsync(message);
            logger.LogInformation("Email change confirmation sent to {Email}.", newEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not send email change confirmation to {Email}.", newEmail);
            throw;
        }
    }

    private string BuildBody(string newEmail, string token, DateTime expiresAt)
    {
        var confirmation = string.IsNullOrWhiteSpace(options.ConfirmationUrl)
            ? token
            : $"{options.ConfirmationUrl}{(options.ConfirmationUrl.Contains('?') ? '&' : '?')}" +
              $"email={Uri.EscapeDataString(newEmail)}&token={Uri.EscapeDataString(token)}";

        return $"""
            A request was made to change the TownsSquare account email to {newEmail}.

            Confirm the change using this link/code:

            {confirmation}

            This request expires at {expiresAt:u}. If you did not request this
            change, you can ignore this message.
            """;
    }
}
