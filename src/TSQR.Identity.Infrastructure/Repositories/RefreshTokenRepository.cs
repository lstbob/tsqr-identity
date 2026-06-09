namespace TSQR.Identity.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDatabaseConnection _db;

    public RefreshTokenRepository(IDatabaseConnection db)
    {
        _db = db;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        var conn = await _db.GetConnectionAsync(ct);
        await conn.ExecuteAsync(
            @"INSERT INTO RefreshTokens (Id, UserId, TokenHash, ExpiresAt, CreatedAt)
              VALUES (@Id, @UserId, @TokenHash, @ExpiresAt, @CreatedAt)", token);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        var conn = await _db.GetConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<RefreshToken>(
            "SELECT * FROM RefreshTokens WHERE TokenHash = @Hash", new { Hash = tokenHash });
    }

    public async Task RevokeAsync(string tokenId, CancellationToken ct = default)
    {
        var conn = await _db.GetConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE RefreshTokens SET RevokedAt = @Now WHERE Id = @Id",
            new { Id = tokenId, Now = DateTime.UtcNow });
    }

    public async Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        var conn = await _db.GetConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE RefreshTokens SET RevokedAt = @Now WHERE UserId = @UserId AND RevokedAt IS NULL",
            new { UserId = userId, Now = DateTime.UtcNow });
    }
}
