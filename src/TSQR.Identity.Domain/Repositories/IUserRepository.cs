using TSQR.Identity.Domain.Entities;

namespace TSQR.Identity.Domain.Repositories;

public interface IUserAdminRepository
{
    Task<List<UserProfile>> GetAllAsync(CancellationToken ct = default);
    Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default);
    Task UpdateAsync(UserProfile user, CancellationToken ct = default);
    Task DeleteAsync(string userId, CancellationToken ct = default);
}
