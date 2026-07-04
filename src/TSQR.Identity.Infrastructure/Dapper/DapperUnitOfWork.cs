using Npgsql;

namespace TSQR.Identity.Infrastructure.Dapper;

public sealed class DapperUnitOfWork(string connectionString) : ISqlUnitOfWork, IDisposable
{
    private readonly NpgsqlConnection _connection = new(connectionString);
    private NpgsqlTransaction? _transaction;
    private DapperConnection? _connectionAdapter;
    private bool _disposed;

    public ISqlConnection Connection
    {
        get
        {
            EnsureTransaction();
            return _connectionAdapter!;
        }
    }

    private void EnsureTransaction()
    {
        if (_transaction is not null) return;
        if (_connection.State != System.Data.ConnectionState.Open)
            _connection.Open();
        _transaction = _connection.BeginTransaction();
        _connectionAdapter = new DapperConnection(_connection, _transaction);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        const int maxAttempts = 3;
        const int delayMs = 100;

        for (int attempt = 1; ; attempt++)
        {
            if (_transaction is null) EnsureTransaction();
            try
            {
                await _transaction!.CommitAsync(ct);
                _transaction.Dispose();
                _transaction = null;
                _connectionAdapter = new DapperConnection(_connection, null);
                return 0;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransient(ex))
            {
                try { await _transaction!.RollbackAsync(ct); } catch { }
                _transaction?.Dispose();
                _transaction = null;
                _connectionAdapter = new DapperConnection(_connection, null);
                await Task.Delay(delayMs * attempt, ct);
                EnsureTransaction();
            }
        }
    }

    private static bool IsTransient(Exception ex)
    {
        if (ex is NpgsqlException npgsql) return npgsql.IsTransient;
        if (ex is IOException) return true;
        return false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_transaction is not null)
            {
                try { _transaction.Rollback(); } catch { }
                _transaction.Dispose();
            }
            _connection.Dispose();
            _disposed = true;
        }
    }
}
