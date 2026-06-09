using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Roles;

public record AssignRoleCommand(string UserId, string RoleName);

public class AssignRoleCommandHandler : IInteractor<AssignRoleCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public AssignRoleCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(AssignRoleCommand command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var result = await _userManager.AddToRoleAsync(user, command.RoleName);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.First();
            return new ValidationError(firstError.Code, firstError.Description);
        }

        return Result.Success();
    }
}
