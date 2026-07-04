namespace TSQR.Identity.Infrastructure.Dapper;

public interface ISqlUnitOfWork : IUnitOfWork
{
    ISqlConnection Connection { get; }
}
