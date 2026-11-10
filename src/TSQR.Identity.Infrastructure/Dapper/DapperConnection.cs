using Dapper;
using Npgsql;

namespace TSQR.Identity.Infrastructure.Dapper;

internal sealed class DapperConnection(NpgsqlConnection connection, NpgsqlTransaction? transaction)
    : ISqlConnection
{
    public Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken ct = default) =>
        SqlMapper.QueryAsync<T>(
            connection,
            new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));

    public Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken ct = default) =>
        SqlMapper.QuerySingleOrDefaultAsync<T>(
            connection,
            new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));

    public Task<T> ExecuteScalarAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken ct = default) =>
        SqlMapper.ExecuteScalarAsync<T>(
            connection,
            new CommandDefinition(sql, parameters, transaction, cancellationToken: ct))!;

    public Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken ct = default) =>
        SqlMapper.ExecuteAsync(
            connection,
            new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));

    public void Dispose() { }
}
