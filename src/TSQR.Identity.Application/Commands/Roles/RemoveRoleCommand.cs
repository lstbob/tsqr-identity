using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Roles;

public record RemoveRoleCommand(string UserId, string RoleName);

public class RemoveRoleCommandHandler(IRoleRepository roles, IUnitOfWork uow)
    : IInteractor<RemoveRoleCommand, Result>
{
    public async Task<Result> ExecuteAsync(RemoveRoleCommand command, CancellationToken ct = default)
    {
        await roles.RemoveRoleAsync(command.UserId, command.RoleName, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
