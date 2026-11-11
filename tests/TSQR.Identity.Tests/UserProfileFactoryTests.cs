using TSQR.Common.Errors;
using TSQR.Identity.Domain.Entities;

namespace TSQR.Identity.Tests;

public class UserProfileFactoryTests
{
    [Fact]
    public void Create_ValidInput_SetsProfileFields()
    {
        var before = DateTime.UtcNow;

        var result = UserProfile.Create(
            "user-1",
            "Ada",
            "Lovelace",
            avatarUrl: "avatar.png",
            bio: "Mathematician");

        var after = DateTime.UtcNow;

        Assert.True(result.IsSuccess);
        var profile = result.Value;
        Assert.Equal("user-1", profile.Id);
        Assert.Equal("Ada", profile.FirstName);
        Assert.Equal("Lovelace", profile.LastName);
        Assert.Equal("avatar.png", profile.AvatarUrl);
        Assert.Equal("Mathematician", profile.Bio);
        Assert.InRange(profile.CreatedAt, before, after);
        Assert.Equal(profile.CreatedAt, profile.UpdatedAt);
    }

    [Fact]
    public void Create_BlankId_ReturnsValidationError()
    {
        var result = UserProfile.Create(" ");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("Id", error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_BlankOptionalName_ReturnsValidationError(string name)
    {
        var result = UserProfile.Create("user-1", firstName: name);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("FirstName", error.Code);
    }

    [Fact]
    public void Create_BlankAvatar_ReturnsValidationError()
    {
        var result = UserProfile.Create("user-1", avatarUrl: " ");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("AvatarUrl", error.Code);
    }

    [Fact]
    public void Create_PreservesSuppliedTimestamps()
    {
        var createdAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var result = UserProfile.Create(
            "user-1",
            createdAt: createdAt,
            updatedAt: createdAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(createdAt, result.Value.CreatedAt);
        Assert.Equal(createdAt, result.Value.UpdatedAt);
    }
}
