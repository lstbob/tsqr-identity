using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Users;

public record UpdateUserCommand(string UserId, string? FirstName, string? LastName);

public class UpdateUserCommandHandler(
    IUserAdminRepository users,
    IUnitOfWork uow,
    ICurrentUser currentUser)
    : IInteractor<UpdateUserCommand, Result>
{
    public async Task<Result> ExecuteAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(actorId))
            return new ValidationError("Actor", "An authenticated user is required.");

        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var updateResult = user.UpdateProfile(
            command.FirstName,
            command.LastName,
            updatedBy: actorId);
        if (updateResult.IsFailure)
            return updateResult.Error;

        await users.UpdateAsync(user, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
