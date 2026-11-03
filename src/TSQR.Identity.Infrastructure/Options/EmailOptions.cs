namespace TSQR.Identity.Infrastructure.Options;

public sealed class EmailOptions
{
    public string FromAddress { get; set; } = "no-reply@townssquare.local";
    public string FromName { get; set; } = "TownsSquare";
    public string ConfirmationUrl { get; set; } = "http://localhost:3000/portal/profile/confirm-email";

    /// <summary>
    /// Local-development fallback. When enabled, the logging sender writes the
    /// raw token to the application log. Never enable this in production.
    /// </summary>
    public bool LogConfirmationToken { get; set; }

    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public bool SmtpUseSsl { get; set; } = true;
    public string? SmtpUserName { get; set; }
    public string? SmtpPassword { get; set; }
}
