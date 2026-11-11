using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Users;

public record ReinstateUserCommand(string UserId);

public class ReinstateUserCommandHandler(
    IUserAdminRepository users,
    IUnitOfWork uow,
    ICurrentUser currentUser)
    : IInteractor<ReinstateUserCommand, Result>
{
    public async Task<Result> ExecuteAsync(ReinstateUserCommand command, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(actorId))
            return new ValidationError("Actor", "An authenticated user is required.");

        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var reinstateResult = user.Reinstate(actorId);
        if (reinstateResult.IsFailure)
            return reinstateResult.Error;

        await users.UpdateAsync(user, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
