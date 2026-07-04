using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Roles;

public record AssignRoleCommand(string UserId, string RoleName);

public class AssignRoleCommandHandler(IRoleRepository roles, IUnitOfWork uow)
    : IInteractor<AssignRoleCommand, Result>
{
    public async Task<Result> ExecuteAsync(AssignRoleCommand command, CancellationToken ct = default)
    {
        var role = await roles.GetByNameAsync(command.RoleName, ct);
        if (role is null)
            return new NotFoundError(nameof(command.RoleName), "Role not found.");

        await roles.AssignRoleAsync(command.UserId, role.Id, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
