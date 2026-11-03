using System.Net.Mail;

namespace TSQR.Identity.Domain.Entities;

/// <summary>
/// A pending request to move a user's login email to a new address. The raw
/// confirmation token is never stored; only its SHA-256 hash is persisted.
/// </summary>
public sealed class PendingEmailChange
{
    public string Id { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string NewEmail { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsConfirmed { get; private set; }

    private PendingEmailChange() { }

    public static PendingEmailChange Create(
        string userId,
        string newEmail,
        string tokenHash,
        DateTime expiresAt,
        DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id must be provided.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash must be provided.", nameof(tokenHash));

        var normalizedEmail = NormalizeEmail(newEmail);
        var now = createdAt ?? DateTime.UtcNow;
        if (expiresAt <= now)
            throw new ArgumentException("Expiry must be in the future.", nameof(expiresAt));

        return new PendingEmailChange
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = userId,
            NewEmail = normalizedEmail,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            IsConfirmed = false,
        };
    }

    public static PendingEmailChange Rehydrate(
        string id,
        string userId,
        string newEmail,
        string tokenHash,
        DateTime expiresAt,
        DateTime createdAt,
        bool isConfirmed) => new()
        {
            Id = id,
            UserId = userId,
            NewEmail = newEmail,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt,
            IsConfirmed = isConfirmed,
        };

    public bool IsExpired(DateTime utcNow) => ExpiresAt <= utcNow;

    public bool IsActive(DateTime utcNow) => !IsConfirmed && !IsExpired(utcNow);

    public void Confirm()
    {
        if (IsConfirmed)
            throw new InvalidOperationException("The email change has already been confirmed.");

        IsConfirmed = true;
    }

    public static string NormalizeEmail(string? email)
    {
        var trimmed = email?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 || trimmed.Length > 200 || !MailAddress.TryCreate(trimmed, out _))
            throw new ArgumentException("A valid email address is required.", nameof(email));

        return trimmed.ToLowerInvariant();
    }
}
