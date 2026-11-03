namespace TSQR.Identity.Application.Abstractions;

/// <summary>
/// Out-of-band delivery port for email-change confirmation messages. The raw
/// token must only travel through this channel, never through an HTTP response.
/// </summary>
public interface IEmailSender
{
    Task SendEmailChangeConfirmationAsync(
        string newEmail,
        string token,
        DateTime expiresAt,
        CancellationToken ct = default);
}
