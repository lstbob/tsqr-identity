using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;
using TSQR.Identity.Infrastructure.Dapper;

namespace TSQR.Identity.Infrastructure.Repositories;

public sealed class UserAdminRepository(ISqlUnitOfWork uow) : IUserAdminRepository
{
    private const string ProfileColumns =
        "Id::text AS Id, FirstName, LastName, AvatarUrl, Bio, CreatedAt, UpdatedAt, LockoutEnd, " +
        "CreatedBy, UpdatedBy, LockedBy, LockReason, DeletedAt, DeletedBy";

    public async Task<List<UserProfile>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await uow.Connection.QueryAsync<UserProfile>(
            $"SELECT {ProfileColumns} FROM user_profiles " +
            "WHERE DeletedAt IS NULL ORDER BY CreatedAt DESC",
            ct: ct);
        return rows.ToList();
    }

    public async Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        return await uow.Connection.QuerySingleOrDefaultAsync<UserProfile>(
            $"SELECT {ProfileColumns} FROM user_profiles " +
            "WHERE Id = @Id::uuid AND DeletedAt IS NULL",
            new { Id = userId },
            ct);
    }

    public async Task UpdateAsync(UserProfile user, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"UPDATE user_profiles SET FirstName = @FirstName, LastName = @LastName,
              LockoutEnd = @LockoutEnd, UpdatedAt = @UpdatedAt,
              UpdatedBy = @UpdatedBy, LockedBy = @LockedBy, LockReason = @LockReason
              WHERE Id = @Id::uuid AND DeletedAt IS NULL",
            user,
            ct);
    }

    public async Task DeleteAsync(
        string userId,
        string? deletedBy = null,
        CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"UPDATE user_profiles
                 SET DeletedAt = now(),
                     DeletedBy = @DeletedBy,
                     UpdatedAt = now()
               WHERE Id = @Id::uuid AND DeletedAt IS NULL",
            new { Id = userId, DeletedBy = deletedBy },
            ct);
    }
}
