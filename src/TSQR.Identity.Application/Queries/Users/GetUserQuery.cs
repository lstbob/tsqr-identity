using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Queries.Users;

public record GetUserQuery(string UserId);

public class GetUserQueryHandler : IInteractor<GetUserQuery, Result<UserListItem>>
{
    private readonly UserManager<AppUser> _userManager;

    public GetUserQueryHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserListItem>> ExecuteAsync(GetUserQuery command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        return new UserListItem(user.Id, user.Email, user.FirstName, user.LastName, user.UserName, user.CreatedAt);
    }
}
