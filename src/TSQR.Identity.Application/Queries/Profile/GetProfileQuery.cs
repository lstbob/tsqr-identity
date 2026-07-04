using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Queries.Profile;

public record GetProfileQuery(string UserId);

public record ProfileResult(
    string Id,
    string? Email,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    string? Bio,
    string? UserName,
    IList<string> Roles,
    bool IsLockedOut,
    DateTime CreatedAt);

public class GetProfileQueryHandler : IInteractor<GetProfileQuery, Result<ProfileResult>>
{
    private readonly UserManager<AppUser> _userManager;

    public GetProfileQueryHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<ProfileResult>> ExecuteAsync(GetProfileQuery command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var isLocked = await _userManager.IsLockedOutAsync(user);

        return new ProfileResult(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.AvatarUrl,
            user.Bio,
            user.UserName,
            roles,
            isLocked,
            user.CreatedAt);
    }
}
