using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.Identity.Application.Commands.Roles;
using TSQR.Identity.Application.Queries.Roles;

namespace TSQR.Identity.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin")]
public sealed class RolesController : ControllerBase
{
    private readonly IInteractor<CreateRoleCommand, Result> _createRole;
    private readonly IInteractor<GetRolesQuery, Result<List<RoleListItem>>> _getRoles;
    private readonly IInteractor<AssignRoleCommand, Result> _assignRole;
    private readonly IInteractor<RemoveRoleCommand, Result> _removeRole;

    public RolesController(
        IInteractor<CreateRoleCommand, Result> createRole,
        IInteractor<GetRolesQuery, Result<List<RoleListItem>>> getRoles,
        IInteractor<AssignRoleCommand, Result> assignRole,
        IInteractor<RemoveRoleCommand, Result> removeRole)
    {
        _createRole = createRole;
        _getRoles = getRoles;
        _assignRole = assignRole;
        _removeRole = removeRole;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RoleResponse>>> GetAll(CancellationToken ct)
    {
        var result = await _getRoles.ExecuteAsync(new GetRolesQuery(), ct);
        if (result.IsSuccess)
            return Ok(result.Value.Select(r => new RoleResponse(r.Id, r.Name)).ToList());

        return BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Create(CreateRoleRequest request, CancellationToken ct)
    {
        var result = await _createRole.ExecuteAsync(new CreateRoleCommand(request.Name), ct);
        if (result.IsSuccess)
            return Ok();

        return BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message));
    }

    [HttpPost("assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Assign(AssignRoleRequest request, CancellationToken ct)
    {
        var result = await _assignRole.ExecuteAsync(new AssignRoleCommand(request.UserId, request.RoleName), ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("remove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Remove(AssignRoleRequest request, CancellationToken ct)
    {
        var result = await _removeRole.ExecuteAsync(new RemoveRoleCommand(request.UserId, request.RoleName), ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    private ActionResult ToErrorResult(Error error)
    {
        return error switch
        {
            NotFoundError err => NotFound(new ErrorResponse(err.Code, err.Message)),
            _ => BadRequest(new ErrorResponse(error.Code, error.Message)),
        };
    }
}
