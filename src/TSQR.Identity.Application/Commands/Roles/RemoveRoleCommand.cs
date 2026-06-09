using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Roles;

public record RemoveRoleCommand(string UserId, string RoleName);

public class RemoveRoleCommandHandler : IInteractor<RemoveRoleCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public RemoveRoleCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(RemoveRoleCommand command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var result = await _userManager.RemoveFromRoleAsync(user, command.RoleName);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.First();
            return new ValidationError(firstError.Code, firstError.Description);
        }

        return Result.Success();
    }
}
