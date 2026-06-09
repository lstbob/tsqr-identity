using System.Data;

namespace TSQR.Identity.Infrastructure.Data;

public interface IDatabaseConnection : IDisposable
{
    Task<IDbConnection> GetConnectionAsync(CancellationToken ct = default);
}

public sealed class IdentityDbConnection : IDatabaseConnection
{
    private readonly string _connectionString;
    private NpgsqlConnection? _connection;

    public IdentityDbConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        _connection ??= new NpgsqlConnection(_connectionString);
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync(ct);
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
