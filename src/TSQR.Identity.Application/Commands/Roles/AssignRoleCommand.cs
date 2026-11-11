using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Roles;

public record AssignRoleCommand(string UserId, string RoleName);

public class AssignRoleCommandHandler(
    IRoleRepository roles,
    IUnitOfWork uow,
    ICurrentUser currentUser)
    : IInteractor<AssignRoleCommand, Result>
{
    public async Task<Result> ExecuteAsync(AssignRoleCommand command, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(actorId))
            return new ValidationError("Actor", "An authenticated user is required.");

        var role = await roles.GetByNameAsync(command.RoleName, ct);
        if (role is null)
            return new NotFoundError(nameof(command.RoleName), "Role not found.");

        await roles.AssignRoleAsync(command.UserId, role.Id, actorId, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
