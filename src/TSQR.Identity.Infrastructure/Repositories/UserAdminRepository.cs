using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;
using TSQR.Identity.Infrastructure.Dapper;

namespace TSQR.Identity.Infrastructure.Repositories;

public sealed class UserAdminRepository(ISqlUnitOfWork uow) : IUserAdminRepository
{
    public async Task<List<UserProfile>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await uow.Connection.QueryAsync<UserProfile>("SELECT * FROM user_profiles ORDER BY CreatedAt DESC");
        return rows.ToList();
    }

    public async Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        return await uow.Connection.QuerySingleOrDefaultAsync<UserProfile>(
            "SELECT * FROM user_profiles WHERE Id = @Id", new { Id = userId });
    }

    public async Task UpdateAsync(UserProfile user, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"UPDATE user_profiles SET FirstName = @FirstName, LastName = @LastName,
              LockoutEnd = @LockoutEnd, UpdatedAt = @UpdatedAt WHERE Id = @Id", user);
    }

    public async Task DeleteAsync(string userId, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync("DELETE FROM user_profiles WHERE Id = @Id", new { Id = userId });
    }
}
