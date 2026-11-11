using TSQR.Common.Errors;
using TSQR.Identity.Domain.Entities;

namespace TSQR.Identity.Tests;

public class UserProfileDomainTests
{
    [Fact]
    public void UpdateProfile_ChangesFieldsAndUpdatedAt()
    {
        var profile = CreateProfile();
        var before = DateTime.UtcNow;

        var result = profile.UpdateProfile("Grace", "Hopper", "Rear admiral");

        var after = DateTime.UtcNow;

        Assert.True(result.IsSuccess);
        Assert.Equal("Grace", profile.FirstName);
        Assert.Equal("Hopper", profile.LastName);
        Assert.Equal("Rear admiral", profile.Bio);
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

    [Fact]
    public void SetLockoutEnd_FutureDate_SetsLockout()
    {
        var profile = CreateProfile();
        var lockoutEnd = DateTime.UtcNow.AddHours(1);

        var result = profile.SetLockoutEnd(lockoutEnd);

        Assert.True(result.IsSuccess);
        Assert.Equal(lockoutEnd, profile.LockoutEnd);
    }

    [Fact]
    public void SetLockoutEnd_PastDate_ReturnsValidationError()
    {
        var profile = CreateProfile();

        var result = profile.SetLockoutEnd(DateTime.UtcNow.AddMinutes(-1));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("LockoutEnd", error.Code);
        Assert.Null(profile.LockoutEnd);
    }

    [Fact]
    public void SetLockoutEnd_Null_ClearsLockout()
    {
        var profile = CreateProfile();
        Assert.True(profile.SetLockoutEnd(DateTime.UtcNow.AddHours(1)).IsSuccess);

        var result = profile.SetLockoutEnd(null);

        Assert.True(result.IsSuccess);
        Assert.Null(profile.LockoutEnd);
    }

    [Fact]
    public void SetAvatar_ValidUrl_SetsAvatar()
    {
        var profile = CreateProfile();

        var result = profile.SetAvatar("/avatars/user-1.png");

        Assert.True(result.IsSuccess);
        Assert.Equal("/avatars/user-1.png", profile.AvatarUrl);
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
        var result = UserProfile.Create("user-1", "Ada", "Lovelace", bio: "Mathematician");
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
