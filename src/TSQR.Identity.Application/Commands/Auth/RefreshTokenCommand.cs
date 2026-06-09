using Microsoft.AspNetCore.Identity;
using TSQR.Identity.Application.Interfaces;

namespace TSQR.Identity.Application.Commands.Auth;

public record RefreshTokenCommand(string RefreshToken);

public class RefreshTokenCommandHandler : IInteractor<RefreshTokenCommand, Result<LoginResult>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public RefreshTokenCommandHandler(
        UserManager<AppUser> userManager,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepo)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshTokenRepo = refreshTokenRepo;
    }

    public async Task<Result<LoginResult>> ExecuteAsync(RefreshTokenCommand command, CancellationToken ct = default)
    {
        var tokenHash = _jwtService.HashToken(command.RefreshToken);
        var storedToken = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash, ct);

        if (storedToken is null || !storedToken.IsActive)
            return new ValidationError(nameof(command.RefreshToken), "Invalid or expired refresh token.");

        await _refreshTokenRepo.RevokeAsync(storedToken.Id, ct);

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user is null)
            return new NotFoundError("User", "User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user, roles);

        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newTokenHash = _jwtService.HashToken(newRefreshToken);

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        }, ct);

        return new LoginResult(accessToken, expiresAt, newRefreshToken);
    }
}
