using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Users;

public record DeleteUserCommand(string UserId);

public class DeleteUserCommandHandler(
    IUserAdminRepository users,
    IUnitOfWork uow,
    ICurrentUser currentUser)
    : IInteractor<DeleteUserCommand, Result>
{
    public async Task<Result> ExecuteAsync(DeleteUserCommand command, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        await users.DeleteAsync(command.UserId, currentUser.UserId, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
