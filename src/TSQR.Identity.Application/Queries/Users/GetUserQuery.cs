using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Queries.Users;

public record GetUserQuery(string UserId);

public class GetUserQueryHandler(IUserAdminRepository users)
    : IInteractor<GetUserQuery, Result<UserListItem>>
{
    public async Task<Result<UserListItem>> ExecuteAsync(GetUserQuery command, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        return new UserListItem(user.Id, user.FirstName, user.LastName, user.CreatedAt);
    }
}
