using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Auth;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName);

public class RegisterCommandHandler : IInteractor<RegisterCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public RegisterCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> ExecuteAsync(RegisterCommand command, CancellationToken ct = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser is not null)
            return new ValidationError(nameof(command.Email), "Email is already registered.");

        var user = new AppUser
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName
        };

        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.First();
            return new ValidationError(firstError.Code, firstError.Description);
        }

        await _userManager.AddToRoleAsync(user, "Member");

        return Result.Success();
    }
}
