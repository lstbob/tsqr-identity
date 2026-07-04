using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Queries.Users;

public record GetCurrentUserQuery(string UserId);

public record CurrentUserResult(
    string Id,
    string? FirstName,
    string? LastName,
    List<string> Roles,
    DateTime CreatedAt);

public class GetCurrentUserQueryHandler(IUserAdminRepository users, IRoleRepository roles)
    : IInteractor<GetCurrentUserQuery, Result<CurrentUserResult>>
{
    public async Task<Result<CurrentUserResult>> ExecuteAsync(GetCurrentUserQuery command, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return new NotFoundError(nameof(command.UserId), "User not found.");

        var userRoles = await roles.GetUserRolesAsync(command.UserId, ct);

        return new CurrentUserResult(user.Id, user.FirstName, user.LastName, userRoles, user.CreatedAt);
    }
}
