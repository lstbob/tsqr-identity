using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Queries.Users;

public record GetCurrentUserQuery(string UserId);

public record CurrentUserResult(
    string Id,
    string? Email,
    string? FirstName,
    string? LastName,
    string? UserName,
    IList<string> Roles,
    bool IsLockedOut,
    DateTime CreatedAt);

public class GetCurrentUserQueryHandler : IInteractor<GetCurrentUserQuery, Result<CurrentUserResult>>
{
    private readonly UserManager<AppUser> _userManager;

    public GetCurrentUserQueryHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<CurrentUserResult>> ExecuteAsync(GetCurrentUserQuery command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var isLocked = await _userManager.IsLockedOutAsync(user);

        return new CurrentUserResult(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.UserName,
            roles,
            isLocked,
            user.CreatedAt);
    }
}
