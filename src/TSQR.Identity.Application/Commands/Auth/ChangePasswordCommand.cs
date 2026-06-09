using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Auth;

public record ChangePasswordCommand(string UserId, string CurrentPassword, string NewPassword);

public class ChangePasswordCommandHandler : IInteractor<ChangePasswordCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public ChangePasswordCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(ChangePasswordCommand command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var result = await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.First();
            return new ValidationError(firstError.Code, firstError.Description);
        }

        return Result.Success();
    }
}
