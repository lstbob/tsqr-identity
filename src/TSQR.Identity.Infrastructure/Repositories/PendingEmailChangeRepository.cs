using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;
using TSQR.Identity.Infrastructure.Dapper;

namespace TSQR.Identity.Infrastructure.Repositories;

public sealed class PendingEmailChangeRepository(ISqlUnitOfWork uow) : IPendingEmailChangeRepository
{
    public async Task AddAsync(PendingEmailChange change, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            @"INSERT INTO pending_email_changes
                  (Id, UserId, NewEmail, TokenHash, ExpiresAt, CreatedAt, IsConfirmed)
              VALUES
                  (@Id, @UserId, @NewEmail, @TokenHash, @ExpiresAt, @CreatedAt, @IsConfirmed)",
            new
            {
                change.Id,
                change.UserId,
                change.NewEmail,
                change.TokenHash,
                change.ExpiresAt,
                change.CreatedAt,
                change.IsConfirmed,
            });
    }

    public async Task<PendingEmailChange?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken ct = default)
    {
        var row = await uow.Connection.QuerySingleOrDefaultAsync<PendingEmailChangeRow>(
            @"SELECT Id, UserId, NewEmail, TokenHash, ExpiresAt, CreatedAt, IsConfirmed
              FROM pending_email_changes
              WHERE TokenHash = @TokenHash",
            new { TokenHash = tokenHash });

        return row is null
            ? null
            : PendingEmailChange.Rehydrate(
                row.Id,
                row.UserId,
                row.NewEmail,
                row.TokenHash,
                row.ExpiresAt,
                row.CreatedAt,
                row.IsConfirmed);
    }

    public async Task DeleteUnconfirmedForUserAsync(
        string userId,
        CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            "DELETE FROM pending_email_changes WHERE UserId = @UserId AND IsConfirmed = FALSE",
            new { UserId = userId });
    }

    public async Task UpdateAsync(PendingEmailChange change, CancellationToken ct = default)
    {
        await uow.Connection.ExecuteAsync(
            "UPDATE pending_email_changes SET IsConfirmed = @IsConfirmed WHERE Id = @Id",
            new { change.Id, change.IsConfirmed });
    }

    private sealed record PendingEmailChangeRow(
        string Id,
        string UserId,
        string NewEmail,
        string TokenHash,
        DateTime ExpiresAt,
        DateTime CreatedAt,
        bool IsConfirmed);
}
