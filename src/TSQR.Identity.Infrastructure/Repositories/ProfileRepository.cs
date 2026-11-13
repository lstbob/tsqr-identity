using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;
using TSQR.Identity.Infrastructure.Dapper;

namespace TSQR.Identity.Infrastructure.Repositories;

public sealed class ProfileRepository(ISqlUnitOfWork uow) : IProfileRepository
{
    private const string ProfileColumns =
        "Id::text AS Id, FirstName, LastName, AvatarUrl, Bio, CreatedAt, UpdatedAt, LockoutEnd, " +
        "CreatedBy, UpdatedBy, LockedBy, LockReason, DeletedAt, DeletedBy";

    public async Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        return await uow.Connection.QuerySingleOrDefaultAsync<UserProfile>(
            $"SELECT {ProfileColumns} FROM user_profiles WHERE Id = @Id::uuid AND DeletedAt IS NULL",
            new { Id = userId },
            ct);
    }

    public async Task<bool> ExistsAsync(string userId, CancellationToken ct = default)
    {
        var count = await uow.Connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM user_profiles WHERE Id = @Id::uuid AND DeletedAt IS NULL",
            new { Id = userId },
            ct);
        return count > 0;
    }

    public async Task CreateAsync(UserProfile profile, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"INSERT INTO user_profiles
                  (Id, FirstName, LastName, AvatarUrl, Bio, CreatedAt, UpdatedAt, LockoutEnd,
                   CreatedBy, UpdatedBy, LockedBy, LockReason)
              VALUES
                  (@Id::uuid, @FirstName, @LastName, @AvatarUrl, @Bio, @CreatedAt, @UpdatedAt, @LockoutEnd,
                   @CreatedBy, @UpdatedBy, @LockedBy, @LockReason)
              ON CONFLICT (Id) DO NOTHING",
            profile,
            ct);
    }

    public async Task UpsertAsync(UserProfile profile, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"INSERT INTO user_profiles
                  (Id, FirstName, LastName, AvatarUrl, Bio, CreatedAt, UpdatedAt, LockoutEnd,
                   CreatedBy, UpdatedBy, LockedBy, LockReason)
              VALUES
                  (@Id::uuid, @FirstName, @LastName, @AvatarUrl, @Bio, @CreatedAt, @UpdatedAt, @LockoutEnd,
                   @CreatedBy, @UpdatedBy, @LockedBy, @LockReason)
              ON CONFLICT (Id) DO UPDATE SET
                  FirstName = COALESCE(EXCLUDED.FirstName, user_profiles.FirstName),
                  LastName = COALESCE(EXCLUDED.LastName, user_profiles.LastName),
                  AvatarUrl = COALESCE(EXCLUDED.AvatarUrl, user_profiles.AvatarUrl),
                  Bio = COALESCE(EXCLUDED.Bio, user_profiles.Bio),
                  UpdatedAt = EXCLUDED.UpdatedAt,
                  LockoutEnd = COALESCE(EXCLUDED.LockoutEnd, user_profiles.LockoutEnd),
                  CreatedBy = COALESCE(EXCLUDED.CreatedBy, user_profiles.CreatedBy),
                  UpdatedBy = COALESCE(EXCLUDED.UpdatedBy, user_profiles.UpdatedBy),
                  LockedBy = COALESCE(EXCLUDED.LockedBy, user_profiles.LockedBy),
                  LockReason = COALESCE(EXCLUDED.LockReason, user_profiles.LockReason)
              WHERE user_profiles.DeletedAt IS NULL",
            profile,
            ct);
    }

    public async Task UpdateAsync(UserProfile profile, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"UPDATE user_profiles SET FirstName = @FirstName, LastName = @LastName,
              AvatarUrl = @AvatarUrl, Bio = @Bio, UpdatedAt = @UpdatedAt, LockoutEnd = @LockoutEnd,
              UpdatedBy = @UpdatedBy, LockedBy = @LockedBy, LockReason = @LockReason
              WHERE Id = @Id::uuid AND DeletedAt IS NULL",
            profile,
            ct);
    }
}
