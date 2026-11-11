using Dapper;
using TSQR.Identity.Domain.Entities;

namespace TSQR.Identity.Tests;

public class DapperMappingTests
{
    [Fact]
    public void DefaultTypeMap_CanSetPrivateSetters()
    {
        var profile = UserProfile.Create("user-1").Value;
        var map = new DefaultTypeMap(typeof(UserProfile));

        var firstName = map.GetMember("FirstName");
        var lockoutEnd = map.GetMember("LockoutEnd");
        var createdBy = map.GetMember("CreatedBy");
        var updatedBy = map.GetMember("UpdatedBy");
        var lockedBy = map.GetMember("LockedBy");
        var lockReason = map.GetMember("LockReason");

        Assert.NotNull(firstName);
        Assert.NotNull(lockoutEnd);
        Assert.NotNull(createdBy);
        Assert.NotNull(updatedBy);
        Assert.NotNull(lockedBy);
        Assert.NotNull(lockReason);

        firstName.Property!.SetValue(profile, "Grace");
        lockoutEnd.Property!.SetValue(profile, DateTime.UtcNow.AddHours(1));
        createdBy.Property!.SetValue(profile, "creator-1");
        updatedBy.Property!.SetValue(profile, "admin-1");
        lockedBy.Property!.SetValue(profile, "admin-1");
        lockReason.Property!.SetValue(profile, "Abuse");

        Assert.Equal("Grace", profile.FirstName);
        Assert.NotNull(profile.LockoutEnd);
        Assert.Equal("creator-1", profile.CreatedBy);
        Assert.Equal("admin-1", profile.UpdatedBy);
        Assert.Equal("admin-1", profile.LockedBy);
        Assert.Equal("Abuse", profile.LockReason);
    }

    [Fact]
    public void DefaultTypeMap_CanSetRolePrivateSetters()
    {
        var role = Role.Create("Admin").Value;
        var map = new DefaultTypeMap(typeof(Role));

        var name = map.GetMember("Name");
        var normalizedName = map.GetMember("NormalizedName");

        Assert.NotNull(name);
        Assert.NotNull(normalizedName);

        name.Property!.SetValue(role, "Coordinator");
        normalizedName.Property!.SetValue(role, "COORDINATOR");

        Assert.Equal("Coordinator", role.Name);
        Assert.Equal("COORDINATOR", role.NormalizedName);
    }
}
