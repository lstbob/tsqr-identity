namespace TSQR.Identity.Infrastructure.Dapper;

public interface ISqlConnection : IDisposable
{
    Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken ct = default);

    Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken ct = default);

    Task<T> ExecuteScalarAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken ct = default);

    Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken ct = default);
}
