using TSQR.Identity.Domain.Entities;

namespace TSQR.Identity.Domain.Repositories;

public interface IProfileRepository
{
    Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default);
    Task<bool> ExistsAsync(string userId, CancellationToken ct = default);
    Task CreateAsync(UserProfile profile, CancellationToken ct = default);
    Task UpsertAsync(UserProfile profile, CancellationToken ct = default);
    Task UpdateAsync(UserProfile profile, CancellationToken ct = default);
}
