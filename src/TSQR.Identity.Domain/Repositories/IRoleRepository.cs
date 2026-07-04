using TSQR.Identity.Domain.Entities;

namespace TSQR.Identity.Domain.Repositories;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync(CancellationToken ct = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task CreateAsync(Role role, CancellationToken ct = default);
    Task DeleteAsync(string roleId, CancellationToken ct = default);
    Task<bool> UserHasRoleAsync(string userId, string roleName, CancellationToken ct = default);
    Task AssignRoleAsync(string userId, string roleId, CancellationToken ct = default);
    Task RemoveRoleAsync(string userId, string roleName, CancellationToken ct = default);
    Task<List<string>> GetUserRolesAsync(string userId, CancellationToken ct = default);
}
