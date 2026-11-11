using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Roles;

public record CreateRoleCommand(string RoleName);

public class CreateRoleCommandHandler(IRoleRepository roles, IUnitOfWork uow)
    : IInteractor<CreateRoleCommand, Result>
{
    public async Task<Result> ExecuteAsync(CreateRoleCommand command, CancellationToken ct = default)
    {
        var roleResult = Role.Create(command.RoleName);
        if (roleResult.IsFailure)
            return roleResult.Error;

        var role = roleResult.Value;
        var exists = await roles.GetByNameAsync(role.Name, ct);
        if (exists is not null)
            return new ValidationError(nameof(command.RoleName), "Role already exists.");

        await roles.CreateAsync(role, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
