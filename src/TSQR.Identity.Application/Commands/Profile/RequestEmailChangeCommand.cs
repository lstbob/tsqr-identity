using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Application.Security;

namespace TSQR.Identity.Application.Commands.Profile;

public record RequestEmailChangeCommand(string UserId, string NewEmail);

public class RequestEmailChangeCommandHandler(
    IPendingEmailChangeRepository pendingChanges,
    IProfileRepository profiles,
    IEmailSender emailSender,
    IUnitOfWork uow) : IInteractor<RequestEmailChangeCommand, Result>
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

    public async Task<Result> ExecuteAsync(RequestEmailChangeCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserId))
            return new ValidationError("UserId", "A user id is required.");

        if (!EmailAddress.TryNormalize(command.NewEmail, out var newEmail))
            return new ValidationError("NewEmail", "A valid email address is required.");

        var now = DateTime.UtcNow;
        var token = EmailChangeToken.Generate();
        var pending = PendingEmailChange.Create(
            command.UserId,
            newEmail,
            EmailChangeToken.Hash(token),
            now.Add(TokenLifetime),
            now);

        // The profile row owns the foreign key used by pending_email_changes.
        // A user may reach this endpoint before ever saving profile details.
        // CreateAsync is ON CONFLICT DO NOTHING, so no read-before-write race.
        var profileResult = UserProfile.Create(
            command.UserId,
            createdAt: now,
            updatedAt: now);

        if (profileResult.IsFailure)
            return profileResult.Error;

        await profiles.CreateAsync(profileResult.Value, ct);

        await pendingChanges.DeleteUnconfirmedForUserAsync(command.UserId, ct);
        await pendingChanges.AddAsync(pending, ct);
        await uow.SaveChangesAsync(ct);

        try
        {
            await emailSender.SendEmailChangeConfirmationAsync(
                pending.NewEmail, token, pending.ExpiresAt, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new DomainError(
                "EmailDeliveryFailed",
                "Could not send the confirmation email. Please try again.");
        }

        return Result.Success();
    }
}
