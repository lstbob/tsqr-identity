namespace TSQR.Identity.Application.Commands.Profile;

public record ChangeEmailCommand(string UserId, string NewEmail, string Token);

public class ChangeEmailCommandHandler : IInteractor<ChangeEmailCommand, Result>
{
    public Task<Result> ExecuteAsync(ChangeEmailCommand command, CancellationToken ct = default)
    {
        return Task.FromResult(Result.Success());
    }
}
