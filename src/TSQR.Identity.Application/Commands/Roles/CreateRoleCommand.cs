using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Commands.Roles;

public record CreateRoleCommand(string RoleName);

public class CreateRoleCommandHandler : IInteractor<CreateRoleCommand, Result>
{
    private readonly RoleManager<Role> _roleManager;

    public CreateRoleCommandHandler(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result> ExecuteAsync(CreateRoleCommand command, CancellationToken ct = default)
    {
        var exists = await _roleManager.RoleExistsAsync(command.RoleName);
        if (exists)
            return new ValidationError(nameof(command.RoleName), "Role already exists.");

        var role = new Role(command.RoleName);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.First();
            return new ValidationError(firstError.Code, firstError.Description);
        }

        return Result.Success();
    }
}
