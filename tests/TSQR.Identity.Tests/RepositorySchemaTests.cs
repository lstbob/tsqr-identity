using TSQR.Identity.Domain;
using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Infrastructure.Dapper;
using TSQR.Identity.Infrastructure.Repositories;

namespace TSQR.Identity.Tests;

public class RepositorySchemaTests
{
    [Fact]
    public async Task ProfileRepository_ReadsExplicitColumnsAndFiltersDeletedRows()
    {
        var connection = new RecordingSqlConnection();
        var repository = new ProfileRepository(new FakeSqlUnitOfWork(connection));

        await repository.GetByIdAsync("11111111-1111-1111-1111-111111111111");

        Assert.Contains("Id::text AS Id", connection.LastSql);
        Assert.Contains("DeletedAt IS NULL", connection.LastSql);
        Assert.DoesNotContain("SELECT *", connection.LastSql);
        Assert.Contains("@Id::uuid", connection.LastSql);
    }

    [Fact]
    public async Task RoleRepository_GetAllLoadsPermissions()
    {
        var connection = new RecordingSqlConnection();
        var repository = new RoleRepository(new FakeSqlUnitOfWork(connection));

        await repository.GetAllAsync();

        Assert.Contains("roles_permissions", connection.LastSql);
    }

    [Fact]
    public async Task RoleRepository_CreatePersistsPermissions()
    {
        var connection = new RecordingSqlConnection();
        var repository = new RoleRepository(new FakeSqlUnitOfWork(connection));
        var role = Role.Create("Probe", ["tools.manage"]).Value;

        await repository.CreateAsync(role);

        Assert.Contains("roles_permissions", connection.LastSql);
        Assert.Contains("@Permission", connection.LastSql);
    }

    [Fact]
    public async Task UserAdminRepository_GetAllFiltersDeletedRows()
    {
        var connection = new RecordingSqlConnection();
        var repository = new UserAdminRepository(new FakeSqlUnitOfWork(connection));

        await repository.GetAllAsync();

        Assert.Contains("DeletedAt IS NULL", connection.LastSql);
        Assert.DoesNotContain("SELECT *", connection.LastSql);
    }

    [Fact]
    public async Task UserAdminRepository_DeleteSoftDeletesWithActor()
    {
        var connection = new RecordingSqlConnection();
        var repository = new UserAdminRepository(new FakeSqlUnitOfWork(connection));

        await repository.DeleteAsync(
            "11111111-1111-1111-1111-111111111111",
            "admin-1");

        Assert.Contains("UPDATE user_profiles", connection.LastSql);
        Assert.Contains("DeletedAt = now()", connection.LastSql);
        Assert.Contains("DeletedBy = @DeletedBy", connection.LastSql);
        Assert.DoesNotContain("DELETE FROM user_profiles", connection.LastSql);
    }

    private sealed class RecordingSqlConnection : ISqlConnection
    {
        public string LastSql { get; private set; } = string.Empty;

        public Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken ct = default)
        {
            LastSql = sql;
            if (typeof(T) == typeof(Role))
                return Task.FromResult(new[] { Role.Create("Admin").Value }.Cast<T>());

            return Task.FromResult(Enumerable.Empty<T>());
        }

        public Task<T?> QuerySingleOrDefaultAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken ct = default)
        {
            LastSql = sql;
            return Task.FromResult(default(T?));
        }

        public Task<T> ExecuteScalarAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken ct = default)
        {
            LastSql = sql;
            return Task.FromResult(default(T)!);
        }

        public Task<int> ExecuteAsync(
            string sql,
            object? parameters = null,
            CancellationToken ct = default)
        {
            LastSql = sql;
            return Task.FromResult(0);
        }

        public void Dispose() { }
    }

    private sealed class FakeSqlUnitOfWork(RecordingSqlConnection connection) : ISqlUnitOfWork
    {
        public ISqlConnection Connection { get; } = connection;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }
}
