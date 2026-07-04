using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Profile;

public record RequestEmailChangeCommand(string UserId, string NewEmail);

public class RequestEmailChangeCommandHandler : IInteractor<RequestEmailChangeCommand, Result<string>>
{
    private readonly UserManager<AppUser> _userManager;

    public RequestEmailChangeCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<string>> ExecuteAsync(RequestEmailChangeCommand command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var existing = await _userManager.FindByEmailAsync(command.NewEmail);
        if (existing is not null && existing.Id != command.UserId)
            return new ValidationError(nameof(command.NewEmail), "Email is already in use.");

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, command.NewEmail);
        return Result<string>.Success(token);
    }
}
