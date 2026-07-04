using Npgsql;

namespace TSQR.Identity.Infrastructure.Dapper;

internal sealed class DapperConnection(NpgsqlConnection connection, NpgsqlTransaction? transaction)
    : ISqlConnection
{
    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null) =>
        SqlMapper.QueryAsync<T>(connection, sql, parameters, transaction);

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null) =>
        await SqlMapper.QuerySingleOrDefaultAsync<T>(connection, sql, parameters, transaction);

    public Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null) =>
        SqlMapper.ExecuteScalarAsync<T>(connection, sql, parameters, transaction)!;

    public Task<int> ExecuteAsync(string sql, object? parameters = null) =>
        SqlMapper.ExecuteAsync(connection, sql, parameters, transaction);

    public void Dispose() { }
}
