using TSQR.Identity.Domain;
using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Infrastructure.Dapper;
using TSQR.Identity.Infrastructure.Repositories;

namespace TSQR.Identity.Tests;

public class RepositoryCancellationTests
{
    [Fact]
    public async Task Repositories_ForwardCancellationToken()
    {
        var connection = new RecordingSqlConnection();
        var uow = new FakeSqlUnitOfWork(connection);
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;

        await new ProfileRepository(uow).GetByIdAsync("user-1", ct);
        Assert.Equal(ct, connection.LastToken);

        await new RoleRepository(uow).GetAllAsync(ct);
        Assert.Equal(ct, connection.LastToken);

        await new UserAdminRepository(uow).GetByIdAsync("user-1", ct);
        Assert.Equal(ct, connection.LastToken);

        await new PendingEmailChangeRepository(uow).DeleteUnconfirmedForUserAsync("user-1", ct);
        Assert.Equal(ct, connection.LastToken);
    }

    [Fact]
    public async Task ProfileRepository_CreateAsync_UsesConflictDoNothing()
    {
        var connection = new RecordingSqlConnection();
        var repository = new ProfileRepository(new FakeSqlUnitOfWork(connection));
        using var cts = new CancellationTokenSource();

        await repository.CreateAsync(NewProfile(), cts.Token);

        Assert.Equal(cts.Token, connection.LastToken);
        Assert.Contains("ON CONFLICT (Id) DO NOTHING", connection.LastSql);
    }

    [Fact]
    public async Task ProfileRepository_UpsertAsync_UsesConflictDoUpdateAndCoalesce()
    {
        var connection = new RecordingSqlConnection();
        var repository = new ProfileRepository(new FakeSqlUnitOfWork(connection));
        using var cts = new CancellationTokenSource();

        await repository.UpsertAsync(NewProfile(), cts.Token);

        Assert.Equal(cts.Token, connection.LastToken);
        Assert.Contains("ON CONFLICT (Id) DO UPDATE", connection.LastSql);
        Assert.Contains("COALESCE(EXCLUDED.FirstName, user_profiles.FirstName)", connection.LastSql);
        Assert.Contains("UpdatedAt = EXCLUDED.UpdatedAt", connection.LastSql);
    }

    private static UserProfile NewProfile()
    {
        var result = UserProfile.Create("user-1", "Ada", "Lovelace");
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private sealed class RecordingSqlConnection : ISqlConnection
    {
        public string LastSql { get; private set; } = string.Empty;
        public CancellationToken? LastToken { get; private set; }

        public Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken ct = default)
        {
            Record(sql, ct);
            return Task.FromResult(Enumerable.Empty<T>());
        }

        public Task<T?> QuerySingleOrDefaultAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken ct = default)
        {
            Record(sql, ct);
            return Task.FromResult(default(T?));
        }

        public Task<T> ExecuteScalarAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken ct = default)
        {
            Record(sql, ct);
            return Task.FromResult(default(T)!);
        }

        public Task<int> ExecuteAsync(
            string sql,
            object? parameters = null,
            CancellationToken ct = default)
        {
            Record(sql, ct);
            return Task.FromResult(0);
        }

        public void Dispose() { }

        private void Record(string sql, CancellationToken ct)
        {
            LastSql = sql;
            LastToken = ct;
        }
    }

    private sealed class FakeSqlUnitOfWork(RecordingSqlConnection connection) : ISqlUnitOfWork
    {
        public ISqlConnection Connection { get; } = connection;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }
}
