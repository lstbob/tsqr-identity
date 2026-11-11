using TSQR.Common.Extensions;

namespace TSQR.Identity.Domain.Entities;

public sealed class UserProfile : IAggregateRoot
{
    public static readonly TimeSpan MaxLockoutDuration = TimeSpan.FromDays(365);

    public string Id { get; private set; } = string.Empty;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public string? LockedBy { get; private set; }
    public string? LockReason { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

    public static Result<UserProfile> Create(
        string id,
        string? firstName = null,
        string? lastName = null,
        string? avatarUrl = null,
        string? bio = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? lockoutEnd = null,
        string? createdBy = null,
        string? updatedBy = null,
        string? lockedBy = null,
        string? lockReason = null)
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
            var lockoutResult = ValidateLockoutEnd(lockoutEnd.Value);
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
            LockedBy = lockedBy,
            LockReason = lockReason,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy ?? createdBy,
        };
    }

    public Result UpdateProfile(
        string? firstName = null,
        string? lastName = null,
        string? bio = null,
        string? updatedBy = null)
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

        if (updatedBy is not null)
        {
            var actorResult = updatedBy.Validate(nameof(UpdatedBy));
            if (actorResult.IsFailure)
                return actorResult.Error;

            UpdatedBy = updatedBy;
        }

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SetAvatar(string avatarUrl, string? updatedBy = null)
    {
        var avatarResult = avatarUrl.Validate(nameof(AvatarUrl));
        if (avatarResult.IsFailure)
            return avatarResult.Error;

        if (updatedBy is not null)
        {
            var actorResult = updatedBy.Validate(nameof(UpdatedBy));
            if (actorResult.IsFailure)
                return actorResult.Error;

            UpdatedBy = updatedBy;
        }

        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Suspend(DateTime until, string reason, string lockedBy)
    {
        var lockoutResult = ValidateLockoutEnd(until);
        if (lockoutResult.IsFailure)
            return lockoutResult.Error;

        var reasonResult = reason.Validate(nameof(LockReason));
        if (reasonResult.IsFailure)
            return reasonResult.Error;

        var lockedByResult = lockedBy.Validate(nameof(LockedBy));
        if (lockedByResult.IsFailure)
            return lockedByResult.Error;

        if (LockoutEnd is not null && LockoutEnd > DateTime.UtcNow)
            return new DomainError(nameof(LockoutEnd), "User is already suspended.");

        LockoutEnd = until;
        LockReason = reason;
        LockedBy = lockedBy;
        UpdatedBy = lockedBy;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Reinstate(string updatedBy)
    {
        var actorResult = updatedBy.Validate(nameof(UpdatedBy));
        if (actorResult.IsFailure)
            return actorResult.Error;

        if (LockoutEnd is null)
            return new DomainError(nameof(LockoutEnd), "User is not suspended.");

        LockoutEnd = null;
        LockReason = null;
        LockedBy = null;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    private static Result ValidateLockoutEnd(DateTime until)
    {
        var notPastResult = until.ValidateNotInPast(nameof(LockoutEnd));
        if (notPastResult.IsFailure)
            return notPastResult.Error;

        if (until > DateTime.UtcNow.Add(MaxLockoutDuration))
            return new ValidationError(
                nameof(LockoutEnd),
                $"Lockout end cannot be more than {MaxLockoutDuration.TotalDays:0} days in the future.");

        return Result.Success();
    }
}
