using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Users;

public record LockUserCommand(string UserId, DateTime? LockoutEnd);

public class LockUserCommandHandler(IUserAdminRepository users, IUnitOfWork uow)
    : IInteractor<LockUserCommand, Result>
{
    public async Task<Result> ExecuteAsync(LockUserCommand command, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        user.LockoutEnd = command.LockoutEnd;
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
