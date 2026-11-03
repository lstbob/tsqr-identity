using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Application.Security;

namespace TSQR.Identity.Application.Commands.Profile;

public record ChangeEmailCommand(
    string UserId,
    string NewEmail,
    string Token,
    string AccessToken);

public class ChangeEmailCommandHandler(
    IPendingEmailChangeRepository pendingChanges,
    IAutheoEmailClient autheo,
    IUnitOfWork uow) : IInteractor<ChangeEmailCommand, Result>
{
    public async Task<Result> ExecuteAsync(ChangeEmailCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserId))
            return new ValidationError("UserId", "A user id is required.");

        if (string.IsNullOrWhiteSpace(command.AccessToken))
            return new ValidationError("AccessToken", "Authentication is required.");

        if (!EmailAddress.TryNormalize(command.NewEmail, out var newEmail))
            return new ValidationError("NewEmail", "A valid email address is required.");

        if (string.IsNullOrWhiteSpace(command.Token))
            return InvalidToken();

        var pending = await pendingChanges.GetByTokenHashAsync(
            EmailChangeToken.Hash(command.Token), ct);

        if (pending is null)
            return InvalidToken();

        if (!string.Equals(pending.UserId, command.UserId, StringComparison.Ordinal))
            return InvalidToken();

        if (!string.Equals(pending.NewEmail, newEmail, StringComparison.Ordinal))
            return InvalidToken();

        if (pending.IsConfirmed)
            return new ValidationError("Token", "This email change has already been confirmed.");

        if (pending.IsExpired(DateTime.UtcNow))
            return new ValidationError("Token", "This email change request has expired.");

        // autheo owns Users.Email. Forward the caller's access token so the
        // change is applied to the same authenticated user.
        var update = await autheo.ChangeEmailAsync(command.AccessToken, pending.NewEmail, ct);
        if (update.IsFailure)
            return update;

        pending.Confirm();
        await pendingChanges.UpdateAsync(pending, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static ValidationError InvalidToken() =>
        new("Token", "The email-change token is invalid or has expired.");
}
