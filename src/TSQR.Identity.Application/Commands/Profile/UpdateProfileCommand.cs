using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Profile;

public record UpdateProfileCommand(string UserId, string? FirstName, string? LastName, string? Bio);

public class UpdateProfileCommandHandler : IInteractor<UpdateProfileCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public UpdateProfileCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(UpdateProfileCommand command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        if (command.FirstName is not null) user.FirstName = command.FirstName;
        if (command.LastName is not null) user.LastName = command.LastName;
        if (command.Bio is not null) user.Bio = command.Bio;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.First();
            return new ValidationError(firstError.Code, firstError.Description);
        }

        return Result.Success();
    }
}
