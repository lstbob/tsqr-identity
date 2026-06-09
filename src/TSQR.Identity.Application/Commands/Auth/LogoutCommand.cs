namespace TSQR.Identity.Application.Commands.Auth;

public record LogoutCommand(string UserId);

public class LogoutCommandHandler : IInteractor<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepo)
    {
        _refreshTokenRepo = refreshTokenRepo;
    }

    public async Task<Result> ExecuteAsync(LogoutCommand command, CancellationToken ct = default)
    {
        await _refreshTokenRepo.RevokeAllForUserAsync(command.UserId, ct);
        return Result.Success();
    }
}
