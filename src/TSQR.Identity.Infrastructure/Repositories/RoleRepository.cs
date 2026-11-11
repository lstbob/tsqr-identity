using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;
using TSQR.Identity.Infrastructure.Dapper;

namespace TSQR.Identity.Infrastructure.Repositories;

public sealed class RoleRepository(ISqlUnitOfWork uow) : IRoleRepository
{
    public async Task<List<Role>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await uow.Connection.QueryAsync<Role>(
            "SELECT * FROM roles ORDER BY Name",
            ct: ct);
        return rows.ToList();
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await uow.Connection.QuerySingleOrDefaultAsync<Role>(
            "SELECT * FROM roles WHERE NormalizedName = @Name",
            new { Name = name.ToUpperInvariant() },
            ct);
    }

    public async Task CreateAsync(Role role, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            "INSERT INTO roles (Id, Name, NormalizedName) VALUES (@Id, @Name, @NormalizedName)",
            role,
            ct);
    }

    public async Task DeleteAsync(string roleId, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            "DELETE FROM roles WHERE Id = @Id",
            new { Id = roleId },
            ct);
    }

    public async Task<bool> UserHasRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        var count = await uow.Connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM user_roles ur
              INNER JOIN roles r ON r.Id = ur.RoleId
              WHERE ur.UserId = @UserId AND r.NormalizedName = @Name",
            new { UserId = userId, Name = roleName.ToUpperInvariant() },
            ct);
        return count > 0;
    }

    public async Task AssignRoleAsync(
        string userId,
        string roleId,
        string assignedBy,
        CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"INSERT INTO user_roles (UserId, RoleId, AssignedAt, AssignedBy)
              VALUES (@UserId, @RoleId, now(), @AssignedBy)
              ON CONFLICT (UserId, RoleId) DO UPDATE SET
                  AssignedAt = now(),
                  AssignedBy = EXCLUDED.AssignedBy",
            new { UserId = userId, RoleId = roleId, AssignedBy = assignedBy },
            ct);
    }

    public async Task RemoveRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"DELETE FROM user_roles WHERE UserId = @UserId AND RoleId =
              (SELECT Id FROM roles WHERE NormalizedName = @Name)",
            new { UserId = userId, Name = roleName.ToUpperInvariant() },
            ct);
    }

    public async Task<List<string>> GetUserRolesAsync(string userId, CancellationToken ct = default)
    {
        var rows = await uow.Connection.QueryAsync<string>(
            @"SELECT r.Name FROM roles r
              INNER JOIN user_roles ur ON ur.RoleId = r.Id
              WHERE ur.UserId = @UserId",
            new { UserId = userId },
            ct);
        return rows.ToList();
    }
}
