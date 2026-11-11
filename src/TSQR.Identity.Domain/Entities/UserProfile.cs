using TSQR.Common.Extensions;

namespace TSQR.Identity.Domain.Entities;

public sealed class UserProfile : IAggregateRoot
{
    public string Id { get; private set; } = string.Empty;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LockoutEnd { get; private set; }

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

        if (firstName is not null)
        {
            var firstNameResult = firstName.Validate(nameof(FirstName));
            if (firstNameResult.IsFailure)
                return firstNameResult.Error;
        }

        if (lastName is not null)
        {
            var lastNameResult = lastName.Validate(nameof(LastName));
            if (lastNameResult.IsFailure)
                return lastNameResult.Error;
        }

        if (avatarUrl is not null)
        {
            var avatarResult = avatarUrl.Validate(nameof(AvatarUrl));
            if (avatarResult.IsFailure)
                return avatarResult.Error;
        }

        if (lockoutEnd is not null)
        {
            var lockoutResult = lockoutEnd.Value.ValidateNotInPast(nameof(LockoutEnd));
            if (lockoutResult.IsFailure)
                return lockoutResult.Error;
        }

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

    public Result UpdateProfile(
        string? firstName = null,
        string? lastName = null,
        string? bio = null)
    {
        if (firstName is not null)
        {
            var firstNameResult = firstName.Validate(nameof(FirstName));
            if (firstNameResult.IsFailure)
                return firstNameResult.Error;

            FirstName = firstName;
        }

        if (lastName is not null)
        {
            var lastNameResult = lastName.Validate(nameof(LastName));
            if (lastNameResult.IsFailure)
                return lastNameResult.Error;

            LastName = lastName;
        }

        if (bio is not null)
            Bio = bio;

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SetAvatar(string avatarUrl)
    {
        var avatarResult = avatarUrl.Validate(nameof(AvatarUrl));
        if (avatarResult.IsFailure)
            return avatarResult.Error;

        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SetLockoutEnd(DateTime? lockoutEnd)
    {
        if (lockoutEnd is not null)
        {
            var lockoutResult = lockoutEnd.Value.ValidateNotInPast(nameof(LockoutEnd));
            if (lockoutResult.IsFailure)
                return lockoutResult.Error;
        }

        LockoutEnd = lockoutEnd;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
