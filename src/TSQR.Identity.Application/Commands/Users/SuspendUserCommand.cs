using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Users;

public record SuspendUserCommand(string UserId, DateTime Until, string Reason);

public class SuspendUserCommandHandler(
    IUserAdminRepository users,
    IUnitOfWork uow,
    ICurrentUser currentUser)
    : IInteractor<SuspendUserCommand, Result>
{
    public async Task<Result> ExecuteAsync(SuspendUserCommand command, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(actorId))
            return new ValidationError("Actor", "An authenticated user is required.");

        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var suspendResult = user.Suspend(command.Until, command.Reason, actorId);
        if (suspendResult.IsFailure)
            return suspendResult.Error;

        await users.UpdateAsync(user, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
