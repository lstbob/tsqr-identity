using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Infrastructure.IdentityStores;

public sealed class UserStore : IUserStore<AppUser>,
    IUserPasswordStore<AppUser>,
    IUserEmailStore<AppUser>,
    IUserRoleStore<AppUser>,
    IUserLockoutStore<AppUser>,
    IUserClaimStore<AppUser>
{
    private readonly IDatabaseConnection _db;

    public UserStore(IDatabaseConnection db)
    {
        _db = db;
    }

    public async Task<IdentityResult> CreateAsync(AppUser user, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        var sql = @"INSERT INTO AspNetUsers
            (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
             PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed,
             TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, CreatedAt)
            VALUES (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed,
                    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed,
                    @TwoFactorEnabled, @LockoutEnd, @LockoutEnabled, @AccessFailedCount, @FirstName, @LastName, @CreatedAt)";

        await conn.ExecuteAsync(sql, user);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        await conn.ExecuteAsync("DELETE FROM AspNetUsers WHERE Id = @Id", new { user.Id });
        return IdentityResult.Success;
    }

    public async Task<AppUser?> FindByIdAsync(string userId, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<AppUser>(
            "SELECT * FROM AspNetUsers WHERE Id = @Id", new { Id = userId });
    }

    public async Task<AppUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<AppUser>(
            "SELECT * FROM AspNetUsers WHERE NormalizedUserName = @Name",
            new { Name = normalizedUserName });
    }

    public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        var sql = @"UPDATE AspNetUsers SET
            UserName = @UserName, NormalizedUserName = @NormalizedUserName,
            Email = @Email, NormalizedEmail = @NormalizedEmail, EmailConfirmed = @EmailConfirmed,
            PasswordHash = @PasswordHash, SecurityStamp = @SecurityStamp, ConcurrencyStamp = @ConcurrencyStamp,
            PhoneNumber = @PhoneNumber, PhoneNumberConfirmed = @PhoneNumberConfirmed,
            TwoFactorEnabled = @TwoFactorEnabled, LockoutEnd = @LockoutEnd,
            LockoutEnabled = @LockoutEnabled, AccessFailedCount = @AccessFailedCount,
            FirstName = @FirstName, LastName = @LastName
            WHERE Id = @Id";

        await conn.ExecuteAsync(sql, user);
        return IdentityResult.Success;
    }

    public Task<string> GetUserIdAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.Id);
    public Task<string?> GetUserNameAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.UserName);
    public Task<string?> GetNormalizedUserNameAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.NormalizedUserName);
    public Task SetUserNameAsync(AppUser user, string? userName, CancellationToken ct) { user.UserName = userName; return Task.CompletedTask; }
    public Task SetNormalizedUserNameAsync(AppUser user, string? normalizedName, CancellationToken ct) { user.NormalizedUserName = normalizedName; return Task.CompletedTask; }

    // --- IUserPasswordStore ---
    public Task<string?> GetPasswordHashAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.PasswordHash);
    public Task<bool> HasPasswordAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.PasswordHash is not null);
    public Task SetPasswordHashAsync(AppUser user, string? passwordHash, CancellationToken ct) { user.PasswordHash = passwordHash; return Task.CompletedTask; }

    // --- IUserEmailStore ---
    public Task<string?> GetEmailAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.Email);
    public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.EmailConfirmed);
    public Task<string?> GetNormalizedEmailAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.NormalizedEmail);
    public Task SetEmailAsync(AppUser user, string? email, CancellationToken ct) { user.Email = email; return Task.CompletedTask; }
    public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken ct) { user.EmailConfirmed = confirmed; return Task.CompletedTask; }
    public Task SetNormalizedEmailAsync(AppUser user, string? normalizedEmail, CancellationToken ct) { user.NormalizedEmail = normalizedEmail; return Task.CompletedTask; }

    public async Task<AppUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<AppUser>(
            "SELECT * FROM AspNetUsers WHERE NormalizedEmail = @Email",
            new { Email = normalizedEmail });
    }

    // --- IUserRoleStore ---
    public async Task AddToRoleAsync(AppUser user, string roleName, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        var roleId = await conn.QuerySingleOrDefaultAsync<string>(
            "SELECT Id FROM AspNetRoles WHERE NormalizedName = @Name",
            new { Name = roleName.ToUpperInvariant() });

        if (roleId is not null)
            await conn.ExecuteAsync(
                "INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)",
                new { UserId = user.Id, RoleId = roleId });
    }

    public async Task RemoveFromRoleAsync(AppUser user, string roleName, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        await conn.ExecuteAsync(
            @"DELETE FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId =
              (SELECT Id FROM AspNetRoles WHERE NormalizedName = @Name)",
            new { UserId = user.Id, Name = roleName.ToUpperInvariant() });
    }

    public async Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        var roles = await conn.QueryAsync<string>(
            @"SELECT r.Name FROM AspNetRoles r
              INNER JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
              WHERE ur.UserId = @UserId",
            new { UserId = user.Id });

        return roles.ToList();
    }

    public async Task<bool> IsInRoleAsync(AppUser user, string roleName, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM AspNetUserRoles ur
              INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
              WHERE ur.UserId = @UserId AND r.NormalizedName = @Name",
            new { UserId = user.Id, Name = roleName.ToUpperInvariant() });

        return count > 0;
    }

    // --- IUserLockoutStore ---
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(AppUser user, CancellationToken ct) =>
        Task.FromResult(user.LockoutEnd);
    public Task SetLockoutEndDateAsync(AppUser user, DateTimeOffset? lockoutEnd, CancellationToken ct) { user.LockoutEnd = lockoutEnd; return Task.CompletedTask; }
    public Task<int> IncrementAccessFailedCountAsync(AppUser user, CancellationToken ct) { user.AccessFailedCount++; return Task.FromResult(user.AccessFailedCount); }
    public Task ResetAccessFailedCountAsync(AppUser user, CancellationToken ct) { user.AccessFailedCount = 0; return Task.CompletedTask; }
    public Task<int> GetAccessFailedCountAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.AccessFailedCount);
    public Task<bool> GetLockoutEnabledAsync(AppUser user, CancellationToken ct) => Task.FromResult(user.LockoutEnabled);
    public Task SetLockoutEnabledAsync(AppUser user, bool enabled, CancellationToken ct) { user.LockoutEnabled = enabled; return Task.CompletedTask; }

    // --- IUserClaimStore ---
    public async Task<IList<Claim>> GetClaimsAsync(AppUser user, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        var rows = await conn.QueryAsync<(string Type, string Value)>(
            "SELECT ClaimType, ClaimValue FROM AspNetUserClaims WHERE UserId = @UserId",
            new { UserId = user.Id });

        return rows.Select(r => new Claim(r.Type, r.Value)).ToList();
    }

    public async Task AddClaimsAsync(AppUser user, IEnumerable<Claim> claims, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        foreach (var claim in claims)
        {
            await conn.ExecuteAsync(
                "INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue) VALUES (@UserId, @Type, @Value)",
                new { UserId = user.Id, Type = claim.Type, Value = claim.Value });
        }
    }

    public async Task ReplaceClaimAsync(AppUser user, Claim claim, Claim newClaim, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE AspNetUserClaims SET ClaimType = @NewType, ClaimValue = @NewValue WHERE UserId = @UserId AND ClaimType = @Type AND ClaimValue = @Value",
            new { UserId = user.Id, Type = claim.Type, Value = claim.Value, NewType = newClaim.Type, NewValue = newClaim.Value });
    }

    public async Task RemoveClaimsAsync(AppUser user, IEnumerable<Claim> claims, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        foreach (var claim in claims)
        {
            await conn.ExecuteAsync(
                "DELETE FROM AspNetUserClaims WHERE UserId = @UserId AND ClaimType = @Type AND ClaimValue = @Value",
                new { UserId = user.Id, Type = claim.Type, Value = claim.Value });
        }
    }

    // --- IUserRoleStore additions (.NET 10) ---
    public async Task<IList<AppUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        var users = await conn.QueryAsync<AppUser>(
            @"SELECT u.* FROM AspNetUsers u
              INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
              INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
              WHERE r.NormalizedName = @Name",
            new { Name = roleName.ToUpperInvariant() });

        return users.ToList();
    }

    // --- IUserClaimStore additions (.NET 10) ---
    public async Task<IList<AppUser>> GetUsersForClaimAsync(Claim claim, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        var users = await conn.QueryAsync<AppUser>(
            @"SELECT u.* FROM AspNetUsers u
              INNER JOIN AspNetUserClaims c ON c.UserId = u.Id
              WHERE c.ClaimType = @Type AND c.ClaimValue = @Value",
            new { Type = claim.Type, Value = claim.Value });

        return users.ToList();
    }

    public void Dispose()
    {
    }
}
