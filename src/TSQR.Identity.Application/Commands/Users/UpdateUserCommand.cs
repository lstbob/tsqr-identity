using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Users;

public record UpdateUserCommand(string UserId, string? FirstName, string? LastName);

public class UpdateUserCommandHandler(IUserAdminRepository users, IUnitOfWork uow)
    : IInteractor<UpdateUserCommand, Result>
{
    public async Task<Result> ExecuteAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        if (command.FirstName is not null) user.FirstName = command.FirstName;
        if (command.LastName is not null) user.LastName = command.LastName;
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
