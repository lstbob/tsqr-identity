using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Queries.Users;

public record UserListItem(string Id, string? Email, string? FirstName, string? LastName, string? UserName, DateTime CreatedAt);

public record GetUsersQuery;

public class GetUsersQueryHandler : IInteractor<GetUsersQuery, Result<List<UserListItem>>>
{
    private readonly UserManager<AppUser> _userManager;

    public GetUsersQueryHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<List<UserListItem>>> ExecuteAsync(GetUsersQuery command, CancellationToken ct = default)
    {
        var users = await Task.Run(() => _userManager.Users
            .Select(u => new UserListItem(u.Id, u.Email, u.FirstName, u.LastName, u.UserName, u.CreatedAt))
            .ToList(), ct);

        return users;
    }
}
