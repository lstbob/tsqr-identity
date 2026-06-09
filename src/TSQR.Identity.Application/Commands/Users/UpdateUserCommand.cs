using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Users;

public record UpdateUserCommand(string UserId, string? FirstName, string? LastName, string? Email);

public class UpdateUserCommandHandler : IInteractor<UpdateUserCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public UpdateUserCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        if (command.FirstName is not null) user.FirstName = command.FirstName;
        if (command.LastName is not null) user.LastName = command.LastName;
        if (command.Email is not null) user.Email = command.Email;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.First();
            return new ValidationError(firstError.Code, firstError.Description);
        }

        return Result.Success();
    }
}
