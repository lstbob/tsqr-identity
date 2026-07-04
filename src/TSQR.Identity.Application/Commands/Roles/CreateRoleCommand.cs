using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Roles;

public record CreateRoleCommand(string RoleName);

public class CreateRoleCommandHandler(IRoleRepository roles, IUnitOfWork uow)
    : IInteractor<CreateRoleCommand, Result>
{
    public async Task<Result> ExecuteAsync(CreateRoleCommand command, CancellationToken ct = default)
    {
        var exists = await roles.GetByNameAsync(command.RoleName, ct);
        if (exists is not null)
            return new ValidationError(nameof(command.RoleName), "Role already exists.");

        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = command.RoleName,
            NormalizedName = command.RoleName.ToUpperInvariant(),
        };

        await roles.CreateAsync(role, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
