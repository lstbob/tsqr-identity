using TSQR.Common.Errors;
using TSQR.Identity.Domain.Entities;

namespace TSQR.Identity.Tests;

public class RoleDomainTests
{
    [Fact]
    public void Create_ValidName_TrimsAndNormalizes()
    {
        var result = Role.Create("  Admin  ");

        Assert.True(result.IsSuccess);
        var role = result.Value;
        Assert.False(string.IsNullOrWhiteSpace(role.Id));
        Assert.True(Guid.TryParse(role.Id, out _));
        Assert.Equal("Admin", role.Name);
        Assert.Equal("ADMIN", role.NormalizedName);
        Assert.Empty(role.Permissions);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_BlankName_ReturnsValidationError(string name)
    {
        var result = Role.Create(name);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("Name", error.Code);
    }

    [Fact]
    public void Create_TooLongName_ReturnsValidationError()
    {
        var result = Role.Create(new string('a', 101));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("Name", error.Code);
    }

    [Fact]
    public void Create_WithPermissions_AddsPermissions()
    {
        var result = Role.Create("Admin", ["users.lock", "roles.manage"]);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Permissions.Count);
        Assert.Contains("users.lock", result.Value.Permissions);
        Assert.Contains("roles.manage", result.Value.Permissions);
    }

    [Fact]
    public void AddPermission_Duplicate_DoesNotDuplicate()
    {
        var role = CreateRole();

        Assert.True(role.AddPermission("users.lock").IsSuccess);
        Assert.True(role.AddPermission("USERS.LOCK").IsSuccess);

        Assert.Single(role.Permissions);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddPermission_Blank_ReturnsValidationError(string permission)
    {
        var role = CreateRole();

        var result = role.AddPermission(permission);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("permission", error.Code);
        Assert.Empty(role.Permissions);
    }

    [Fact]
    public void RemovePermission_AssignedPermission_RemovesIt()
    {
        var role = CreateRole();
        Assert.True(role.AddPermission("users.lock").IsSuccess);

        var result = role.RemovePermission("USERS.LOCK");

        Assert.True(result.IsSuccess);
        Assert.Empty(role.Permissions);
    }

    [Fact]
    public void RemovePermission_UnassignedPermission_ReturnsDomainError()
    {
        var role = CreateRole();

        var result = role.RemovePermission("users.lock");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("permission", error.Code);
    }

    [Fact]
    public void Rename_ValidName_UpdatesNameAndNormalizedName()
    {
        var role = CreateRole();

        var result = role.Rename("  Coordinator  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Coordinator", role.Name);
        Assert.Equal("COORDINATOR", role.NormalizedName);
    }

    [Fact]
    public void Rename_BlankName_ReturnsValidationErrorAndPreservesName()
    {
        var role = CreateRole();

        var result = role.Rename(" ");

        Assert.True(result.IsFailure);
        Assert.Equal("Admin", role.Name);
        Assert.Equal("ADMIN", role.NormalizedName);
    }

    private static Role CreateRole()
    {
        var result = Role.Create("Admin");
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
