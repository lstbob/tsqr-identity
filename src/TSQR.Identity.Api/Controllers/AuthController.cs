using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.Identity.Application.Commands.Auth;

namespace TSQR.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IInteractor<RegisterCommand, Result> _register;
    private readonly IInteractor<LoginCommand, Result<LoginResult>> _login;
    private readonly IInteractor<RefreshTokenCommand, Result<LoginResult>> _refreshToken;
    private readonly IInteractor<LogoutCommand, Result> _logout;
    private readonly IInteractor<ChangePasswordCommand, Result> _changePassword;

    public AuthController(
        IInteractor<RegisterCommand, Result> register,
        IInteractor<LoginCommand, Result<LoginResult>> login,
        IInteractor<RefreshTokenCommand, Result<LoginResult>> refreshToken,
        IInteractor<LogoutCommand, Result> logout,
        IInteractor<ChangePasswordCommand, Result> changePassword)
    {
        _register = register;
        _login = login;
        _refreshToken = refreshToken;
        _logout = logout;
        _changePassword = changePassword;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName);
        var result = await _register.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok();

        return ToErrorResult(result.Error);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _login.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok(new TokenResponse(result.Value.AccessToken, result.Value.ExpiresAt, result.Value.RefreshToken));

        return ToErrorResult(result.Error);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _refreshToken.ExecuteAsync(command, ct);
        if (result.IsSuccess)
            return Ok(new TokenResponse(result.Value.AccessToken, result.Value.ExpiresAt, result.Value.RefreshToken));

        return ToErrorResult(result.Error);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Logout(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized();

        var command = new LogoutCommand(userId);
        await _logout.ExecuteAsync(command, ct);
        return Ok();
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized();

        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        var result = await _changePassword.ExecuteAsync(command, ct);
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
