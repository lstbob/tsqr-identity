using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;
using TSQR.Identity.Infrastructure.Dapper;

namespace TSQR.Identity.Infrastructure.Repositories;

public sealed class RoleRepository(ISqlUnitOfWork uow) : IRoleRepository
{
    public async Task<List<Role>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await uow.Connection.QueryAsync<Role>(
            "SELECT Id, Name, NormalizedName FROM roles ORDER BY Name",
            ct: ct);
        var roles = rows.ToList();
        await LoadPermissionsAsync(roles, ct);
        return roles;
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var role = await uow.Connection.QuerySingleOrDefaultAsync<Role>(
            "SELECT Id, Name, NormalizedName FROM roles WHERE NormalizedName = @Name",
            new { Name = name.ToUpperInvariant() },
            ct);

        if (role is null)
            return null;

        await LoadPermissionsAsync([role], ct);
        return role;
    }

    public async Task CreateAsync(Role role, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            "INSERT INTO roles (Id, Name, NormalizedName) VALUES (@Id, @Name, @NormalizedName)",
            role,
            ct);

        if (role.Permissions.Count == 0)
            return;

        await uow.Connection.ExecuteAsync(
            @"INSERT INTO roles_permissions (RoleId, Permission)
              VALUES (@RoleId, @Permission)
              ON CONFLICT (RoleId, Permission) DO NOTHING",
            role.Permissions.Select(permission => new
            {
                RoleId = role.Id,
                Permission = permission,
            }),
            ct);
    }

    private async Task LoadPermissionsAsync(
        IReadOnlyCollection<Role> roles,
        CancellationToken ct)
    {
        if (roles.Count == 0)
            return;

        var rows = await uow.Connection.QueryAsync<RolePermissionRow>(
            @"SELECT RoleId, Permission
              FROM roles_permissions
              WHERE RoleId = ANY(@RoleIds)",
            new { RoleIds = roles.Select(role => role.Id).ToArray() },
            ct);

        var byRole = rows
            .GroupBy(row => row.RoleId)
            .ToDictionary(group => group.Key, group => group.Select(row => row.Permission));

        foreach (var role in roles)
        {
            if (!byRole.TryGetValue(role.Id, out var permissions))
                continue;

            foreach (var permission in permissions)
                role.AddPermission(permission);
        }
    }

    private sealed record RolePermissionRow(string RoleId, string Permission);

    public async Task<bool> UserHasRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        var count = await uow.Connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM user_roles ur
              INNER JOIN roles r ON r.Id = ur.RoleId
              WHERE ur.UserId = @UserId::uuid AND r.NormalizedName = @Name",
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
              VALUES (@UserId::uuid, @RoleId, now(), @AssignedBy)
              ON CONFLICT (UserId, RoleId) DO UPDATE SET
                  AssignedAt = now(),
                  AssignedBy = EXCLUDED.AssignedBy",
            new { UserId = userId, RoleId = roleId, AssignedBy = assignedBy },
            ct);
    }

    public async Task RemoveRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"DELETE FROM user_roles WHERE UserId = @UserId::uuid AND RoleId =
              (SELECT Id FROM roles WHERE NormalizedName = @Name)",
            new { UserId = userId, Name = roleName.ToUpperInvariant() },
            ct);
    }

    public async Task<List<string>> GetUserRolesAsync(string userId, CancellationToken ct = default)
    {
        var rows = await uow.Connection.QueryAsync<string>(
            @"SELECT r.Name FROM roles r
              INNER JOIN user_roles ur ON ur.RoleId = r.Id
              WHERE ur.UserId = @UserId::uuid",
            new { UserId = userId },
            ct);
        return rows.ToList();
    }
}
