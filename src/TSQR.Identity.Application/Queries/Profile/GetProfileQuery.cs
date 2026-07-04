using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Queries.Profile;

public record GetProfileQuery(string UserId);

public record ProfileResult(
    string Id,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    string? Bio,
    List<string> Roles,
    DateTime CreatedAt);

public class GetProfileQueryHandler(IProfileRepository profiles, IRoleRepository roles)
    : IInteractor<GetProfileQuery, Result<ProfileResult>>
{
    public async Task<Result<ProfileResult>> ExecuteAsync(GetProfileQuery command, CancellationToken ct = default)
    {
        var profile = await profiles.GetByIdAsync(command.UserId, ct);
        var userRoles = await roles.GetUserRolesAsync(command.UserId, ct);

        if (profile is null)
            return new ProfileResult(
                command.UserId, null, null, null, null, userRoles, DateTime.UtcNow);

        return new ProfileResult(
            profile.Id, profile.FirstName, profile.LastName,
            profile.AvatarUrl, profile.Bio, userRoles, profile.CreatedAt);
    }
}
