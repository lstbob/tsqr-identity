using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Queries.Users;

public record UserListItem(string Id, string? FirstName, string? LastName, DateTime CreatedAt);

public record GetUsersQuery;

public class GetUsersQueryHandler(IUserAdminRepository users)
    : IInteractor<GetUsersQuery, Result<List<UserListItem>>>
{
    public async Task<Result<List<UserListItem>>> ExecuteAsync(GetUsersQuery command, CancellationToken ct = default)
    {
        var all = await users.GetAllAsync(ct);
        var items = all.Select(u => new UserListItem(u.Id, u.FirstName, u.LastName, u.CreatedAt)).ToList();
        return items;
    }
}
