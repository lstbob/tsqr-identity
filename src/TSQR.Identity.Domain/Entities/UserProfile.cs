namespace TSQR.Identity.Domain.Entities;

public sealed class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public static Result<UserProfile> Create(
        string id,
        string? firstName = null,
        string? lastName = null,
        string? avatarUrl = null,
        string? bio = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? lockoutEnd = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return new ValidationError(nameof(Id), "A user id is required.");

        var now = DateTime.UtcNow;

        return new UserProfile
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            AvatarUrl = avatarUrl,
            Bio = bio,
            CreatedAt = createdAt ?? now,
            UpdatedAt = updatedAt ?? now,
            LockoutEnd = lockoutEnd,
        };
    }
}
