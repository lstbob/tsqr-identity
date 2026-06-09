using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Application.Queries.Roles;

public record GetRolesQuery;

public record RoleListItem(string Id, string Name);

public class GetRolesQueryHandler : IInteractor<GetRolesQuery, Result<List<RoleListItem>>>
{
    private readonly RoleManager<Role> _roleManager;

    public GetRolesQueryHandler(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
    }

    public Task<Result<List<RoleListItem>>> ExecuteAsync(GetRolesQuery command, CancellationToken ct = default)
    {
        var roles = _roleManager.Roles
            .Select(r => new RoleListItem(r.Id!, r.Name!))
            .ToList();

        return Task.FromResult(Result<List<RoleListItem>>.Success(roles));
    }
}
