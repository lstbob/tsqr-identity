using Microsoft.AspNetCore.Identity;
using TSQR.Identity.Application.Interfaces;

namespace TSQR.Identity.Application.Commands.Auth;

public record LoginCommand(string Email, string Password);

public record LoginResult(string AccessToken, DateTime ExpiresAt, string RefreshToken);

public class LoginCommandHandler : IInteractor<LoginCommand, Result<LoginResult>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public LoginCommandHandler(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepo)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _refreshTokenRepo = refreshTokenRepo;
    }

    public async Task<Result<LoginResult>> ExecuteAsync(LoginCommand command, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return new NotFoundError(nameof(command.Email), "User not found.");

        if (await _userManager.IsLockedOutAsync(user))
            return new DomainError("AccountLocked", "Account is locked due to multiple failed attempts.");

        if (!await _userManager.CheckPasswordAsync(user, command.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return new ValidationError(nameof(command.Password), "Invalid password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user, roles);

        var refreshToken = _jwtService.GenerateRefreshToken();
        var tokenHash = _jwtService.HashToken(refreshToken);

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        }, ct);

        return new LoginResult(accessToken, expiresAt, refreshToken);
    }
}
