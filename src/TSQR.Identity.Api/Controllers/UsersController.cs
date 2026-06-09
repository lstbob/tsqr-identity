using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.Identity.Application.Commands.Users;
using TSQR.Identity.Application.Queries.Users;

namespace TSQR.Identity.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersController : ControllerBase
{
    private readonly IInteractor<GetUsersQuery, Result<List<UserListItem>>> _getUsers;
    private readonly IInteractor<GetUserQuery, Result<UserListItem>> _getUser;
    private readonly IInteractor<UpdateUserCommand, Result> _updateUser;
    private readonly IInteractor<DeleteUserCommand, Result> _deleteUser;

    public UsersController(
        IInteractor<GetUsersQuery, Result<List<UserListItem>>> getUsers,
        IInteractor<GetUserQuery, Result<UserListItem>> getUser,
        IInteractor<UpdateUserCommand, Result> updateUser,
        IInteractor<DeleteUserCommand, Result> deleteUser)
    {
        _getUsers = getUsers;
        _getUser = getUser;
        _updateUser = updateUser;
        _deleteUser = deleteUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<UserResponse>>> GetAll(CancellationToken ct)
    {
        var result = await _getUsers.ExecuteAsync(new GetUsersQuery(), ct);
        if (result.IsSuccess)
            return Ok(result.Value.Select(u => new UserResponse(u.Id, u.Email, u.FirstName, u.LastName, u.UserName, u.CreatedAt)).ToList());

        return ToErrorResult(result.Error);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> GetById(string id, CancellationToken ct)
    {
        var result = await _getUser.ExecuteAsync(new GetUserQuery(id), ct);
        if (result.IsSuccess)
            return Ok(new UserResponse(result.Value.Id, result.Value.Email, result.Value.FirstName, result.Value.LastName, result.Value.UserName, result.Value.CreatedAt));

        return ToErrorResult(result.Error);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Update(string id, UpdateUserRequest request, CancellationToken ct)
    {
        var command = new UpdateUserCommand(id, request.FirstName, request.LastName, request.Email);
        var result = await _updateUser.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Delete(string id, CancellationToken ct)
    {
        var command = new DeleteUserCommand(id);
        var result = await _deleteUser.ExecuteAsync(command, ct);
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
