using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Queries.Roles;

public record GetRolesQuery;

public record RoleListItem(string Id, string Name);

public class GetRolesQueryHandler(IRoleRepository roles)
    : IInteractor<GetRolesQuery, Result<List<RoleListItem>>>
{
    public async Task<Result<List<RoleListItem>>> ExecuteAsync(GetRolesQuery command, CancellationToken ct = default)
    {
        var all = await roles.GetAllAsync(ct);
        return all.Select(r => new RoleListItem(r.Id, r.Name)).ToList();
    }
}
