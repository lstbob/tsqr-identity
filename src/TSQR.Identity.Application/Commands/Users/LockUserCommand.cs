using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Users;

public record LockUserCommand(string UserId, DateTime? LockoutEnd);

public class LockUserCommandHandler : IInteractor<LockUserCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public LockUserCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(LockUserCommand command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var result = await _userManager.SetLockoutEndDateAsync(user, command.LockoutEnd);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.First();
            return new ValidationError(firstError.Code, firstError.Description);
        }

        return Result.Success();
    }
}
