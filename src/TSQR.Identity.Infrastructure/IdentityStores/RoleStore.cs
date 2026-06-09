using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Infrastructure.IdentityStores;

public sealed class RoleStore : IQueryableRoleStore<Domain.Entities.Role>
{
    private readonly IDatabaseConnection _db;

    public RoleStore(IDatabaseConnection db)
    {
        _db = db;
    }

    public async Task<IdentityResult> CreateAsync(Domain.Entities.Role role, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        await conn.ExecuteAsync(
            "INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp)",
            role);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(Domain.Entities.Role role, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE AspNetRoles SET Name = @Name, NormalizedName = @NormalizedName, ConcurrencyStamp = @ConcurrencyStamp WHERE Id = @Id",
            role);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(Domain.Entities.Role role, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        await conn.ExecuteAsync("DELETE FROM AspNetRoles WHERE Id = @Id", new { role.Id });
        return IdentityResult.Success;
    }

    public async Task<Domain.Entities.Role?> FindByIdAsync(string roleId, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<Domain.Entities.Role>(
            "SELECT * FROM AspNetRoles WHERE Id = @Id", new { Id = roleId });
    }

    public async Task<Domain.Entities.Role?> FindByNameAsync(string normalizedRoleName, CancellationToken ct)
    {
        var conn = await _db.GetConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<Domain.Entities.Role>(
            "SELECT * FROM AspNetRoles WHERE NormalizedName = @Name",
            new { Name = normalizedRoleName });
    }

    public Task<string> GetRoleIdAsync(Domain.Entities.Role role, CancellationToken ct) => Task.FromResult(role.Id);
    public Task<string?> GetRoleNameAsync(Domain.Entities.Role role, CancellationToken ct) => Task.FromResult(role.Name);
    public Task<string?> GetNormalizedRoleNameAsync(Domain.Entities.Role role, CancellationToken ct) => Task.FromResult(role.NormalizedName);
    public Task SetRoleNameAsync(Domain.Entities.Role role, string? roleName, CancellationToken ct) { role.Name = roleName; return Task.CompletedTask; }
    public Task SetNormalizedRoleNameAsync(Domain.Entities.Role role, string? normalizedName, CancellationToken ct) { role.NormalizedName = normalizedName; return Task.CompletedTask; }

    public IQueryable<Domain.Entities.Role> Roles
    {
        get
        {
            var conn = _db.GetConnectionAsync(default).GetAwaiter().GetResult();
            var roles = conn.Query<Domain.Entities.Role>("SELECT * FROM AspNetRoles");
            return roles.AsQueryable();
        }
    }

    public void Dispose()
    {
    }
}
