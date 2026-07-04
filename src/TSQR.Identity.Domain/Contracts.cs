namespace TSQR.Identity.Domain;

public interface IAggregateRoot;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
