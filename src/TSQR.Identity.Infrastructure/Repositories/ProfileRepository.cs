using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;
using TSQR.Identity.Infrastructure.Dapper;

namespace TSQR.Identity.Infrastructure.Repositories;

public sealed class ProfileRepository(ISqlUnitOfWork uow) : IProfileRepository
{
    public async Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        return await uow.Connection.QuerySingleOrDefaultAsync<UserProfile>(
            "SELECT * FROM user_profiles WHERE Id = @Id",
            new { Id = userId },
            ct);
    }

    public async Task<bool> ExistsAsync(string userId, CancellationToken ct = default)
    {
        var count = await uow.Connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM user_profiles WHERE Id = @Id",
            new { Id = userId },
            ct);
        return count > 0;
    }

    public async Task CreateAsync(UserProfile profile, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"INSERT INTO user_profiles (Id, FirstName, LastName, AvatarUrl, Bio, CreatedAt, UpdatedAt, LockoutEnd)
              VALUES (@Id, @FirstName, @LastName, @AvatarUrl, @Bio, @CreatedAt, @UpdatedAt, @LockoutEnd)
              ON CONFLICT (Id) DO NOTHING",
            profile,
            ct);
    }

    public async Task UpsertAsync(UserProfile profile, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"INSERT INTO user_profiles (Id, FirstName, LastName, AvatarUrl, Bio, CreatedAt, UpdatedAt, LockoutEnd)
              VALUES (@Id, @FirstName, @LastName, @AvatarUrl, @Bio, @CreatedAt, @UpdatedAt, @LockoutEnd)
              ON CONFLICT (Id) DO UPDATE SET
                  FirstName = COALESCE(EXCLUDED.FirstName, user_profiles.FirstName),
                  LastName = COALESCE(EXCLUDED.LastName, user_profiles.LastName),
                  AvatarUrl = COALESCE(EXCLUDED.AvatarUrl, user_profiles.AvatarUrl),
                  Bio = COALESCE(EXCLUDED.Bio, user_profiles.Bio),
                  UpdatedAt = EXCLUDED.UpdatedAt,
                  LockoutEnd = COALESCE(EXCLUDED.LockoutEnd, user_profiles.LockoutEnd)",
            profile,
            ct);
    }

    public async Task UpdateAsync(UserProfile profile, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"UPDATE user_profiles SET FirstName = @FirstName, LastName = @LastName,
              AvatarUrl = @AvatarUrl, Bio = @Bio, UpdatedAt = @UpdatedAt, LockoutEnd = @LockoutEnd
              WHERE Id = @Id",
            profile,
            ct);
    }
}
