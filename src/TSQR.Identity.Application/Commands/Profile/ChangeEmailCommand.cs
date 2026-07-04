using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Profile;

public record ChangeEmailCommand(string UserId, string NewEmail, string Token);

public class ChangeEmailCommandHandler : IInteractor<ChangeEmailCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public ChangeEmailCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(ChangeEmailCommand command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var result = await _userManager.ChangeEmailAsync(user, command.NewEmail, command.Token);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.First();
            return new ValidationError(firstError.Code, firstError.Description);
        }

        return Result.Success();
    }
}
