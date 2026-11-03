using TSQR.Identity.Domain.Entities;

namespace TSQR.Identity.Domain.Repositories;

public interface IPendingEmailChangeRepository
{
    Task AddAsync(PendingEmailChange change, CancellationToken ct = default);
    Task<PendingEmailChange?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task DeleteUnconfirmedForUserAsync(string userId, CancellationToken ct = default);
    Task UpdateAsync(PendingEmailChange change, CancellationToken ct = default);
}
