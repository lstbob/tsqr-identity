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

        Assert.NotNull(firstName);
        Assert.NotNull(lockoutEnd);

        firstName.Property!.SetValue(profile, "Grace");
        lockoutEnd.Property!.SetValue(profile, DateTime.UtcNow.AddHours(1));

        Assert.Equal("Grace", profile.FirstName);
        Assert.NotNull(profile.LockoutEnd);
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
