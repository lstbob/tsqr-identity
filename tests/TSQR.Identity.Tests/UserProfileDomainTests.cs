using TSQR.Common.Errors;
using TSQR.Identity.Domain.Entities;

namespace TSQR.Identity.Tests;

public class UserProfileDomainTests
{
    [Fact]
    public void Create_RecordsAuditFields()
    {
        var profile = UserProfile.Create(
            "user-1",
            "Ada",
            "Lovelace",
            createdBy: "creator-1").Value;

        Assert.Equal("creator-1", profile.CreatedBy);
        Assert.Equal("creator-1", profile.UpdatedBy);
    }

    [Fact]
    public void UpdateProfile_ChangesFieldsAndUpdatedAtAndUpdatedBy()
    {
        var profile = CreateProfile();
        var before = DateTime.UtcNow;

        var result = profile.UpdateProfile("Grace", "Hopper", "Rear admiral", "admin-1");

        var after = DateTime.UtcNow;

        Assert.True(result.IsSuccess);
        Assert.Equal("Grace", profile.FirstName);
        Assert.Equal("Hopper", profile.LastName);
        Assert.Equal("Rear admiral", profile.Bio);
        Assert.Equal("admin-1", profile.UpdatedBy);
        Assert.InRange(profile.UpdatedAt, before, after);
    }

    [Fact]
    public void UpdateProfile_NullFields_PreserveExistingValues()
    {
        var profile = CreateProfile();

        var result = profile.UpdateProfile();

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada", profile.FirstName);
        Assert.Equal("Lovelace", profile.LastName);
        Assert.Equal("Mathematician", profile.Bio);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateProfile_BlankFirstName_ReturnsValidationErrorWithoutChangingProfile(string firstName)
    {
        var profile = CreateProfile();

        var result = profile.UpdateProfile(firstName, "Hopper");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("FirstName", error.Code);
        Assert.Equal("Ada", profile.FirstName);
        Assert.Equal("Lovelace", profile.LastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateProfile_BlankUpdatedBy_ReturnsValidationError(string updatedBy)
    {
        var profile = CreateProfile();

        var result = profile.UpdateProfile(updatedBy: updatedBy);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("UpdatedBy", error.Code);
    }

    [Fact]
    public void Suspend_FutureDate_SetsLockoutAndAuditFields()
    {
        var profile = CreateProfile();
        var lockoutEnd = DateTime.UtcNow.AddHours(1);

        var result = profile.Suspend(lockoutEnd, "Repeated abuse", "admin-1");

        Assert.True(result.IsSuccess);
        Assert.Equal(lockoutEnd, profile.LockoutEnd);
        Assert.Equal("Repeated abuse", profile.LockReason);
        Assert.Equal("admin-1", profile.LockedBy);
        Assert.Equal("admin-1", profile.UpdatedBy);
    }

    [Fact]
    public void Suspend_PastDate_ReturnsValidationError()
    {
        var profile = CreateProfile();

        var result = profile.Suspend(DateTime.UtcNow.AddMinutes(-1), "Abuse", "admin-1");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("LockoutEnd", error.Code);
        Assert.Null(profile.LockoutEnd);
    }

    [Fact]
    public void Suspend_TooFarInFuture_ReturnsValidationError()
    {
        var profile = CreateProfile();

        var result = profile.Suspend(
            DateTime.UtcNow.Add(UserProfile.MaxLockoutDuration).AddDays(1),
            "Abuse",
            "admin-1");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("LockoutEnd", error.Code);
        Assert.Null(profile.LockoutEnd);
    }

    [Theory]
    [InlineData("", "admin-1", "LockReason")]
    [InlineData("Abuse", "", "LockedBy")]
    public void Suspend_BlankRequiredField_ReturnsValidationError(
        string reason,
        string lockedBy,
        string expectedCode)
    {
        var profile = CreateProfile();

        var result = profile.Suspend(DateTime.UtcNow.AddHours(1), reason, lockedBy);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(expectedCode, error.Code);
        Assert.Null(profile.LockoutEnd);
    }

    [Fact]
    public void Suspend_AlreadySuspended_ReturnsDomainError()
    {
        var profile = CreateProfile();
        Assert.True(profile.Suspend(DateTime.UtcNow.AddHours(1), "Abuse", "admin-1").IsSuccess);

        var result = profile.Suspend(DateTime.UtcNow.AddHours(2), "More abuse", "admin-2");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("LockoutEnd", error.Code);
    }

    [Fact]
    public void Reinstate_ClearsLockoutAndRecordsActor()
    {
        var profile = CreateProfile();
        Assert.True(profile.Suspend(DateTime.UtcNow.AddHours(1), "Abuse", "admin-1").IsSuccess);

        var result = profile.Reinstate("admin-2");

        Assert.True(result.IsSuccess);
        Assert.Null(profile.LockoutEnd);
        Assert.Null(profile.LockReason);
        Assert.Null(profile.LockedBy);
        Assert.Equal("admin-2", profile.UpdatedBy);
    }

    [Fact]
    public void Reinstate_NotSuspended_ReturnsDomainError()
    {
        var profile = CreateProfile();

        var result = profile.Reinstate("admin-1");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("LockoutEnd", error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Reinstate_BlankActor_ReturnsValidationError(string updatedBy)
    {
        var profile = CreateProfile();
        Assert.True(profile.Suspend(DateTime.UtcNow.AddHours(1), "Abuse", "admin-1").IsSuccess);

        var result = profile.Reinstate(updatedBy);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("UpdatedBy", error.Code);
        Assert.NotNull(profile.LockoutEnd);
    }

    [Fact]
    public void SetAvatar_ValidUrl_SetsAvatarAndUpdatedBy()
    {
        var profile = CreateProfile();

        var result = profile.SetAvatar("/avatars/user-1.png", "admin-1");

        Assert.True(result.IsSuccess);
        Assert.Equal("/avatars/user-1.png", profile.AvatarUrl);
        Assert.Equal("admin-1", profile.UpdatedBy);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void SetAvatar_BlankUrl_ReturnsValidationError(string avatarUrl)
    {
        var profile = CreateProfile();

        var result = profile.SetAvatar(avatarUrl);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("AvatarUrl", error.Code);
        Assert.Null(profile.AvatarUrl);
    }

    private static UserProfile CreateProfile()
    {
        var result = UserProfile.Create(
            "user-1",
            "Ada",
            "Lovelace",
            bio: "Mathematician",
            createdBy: "creator-1");
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
