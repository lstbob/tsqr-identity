using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Profile;

public record UpdateProfileCommand(string UserId, string? FirstName, string? LastName, string? Bio);

public class UpdateProfileCommandHandler(
    IProfileRepository profiles,
    IUnitOfWork uow,
    ICurrentUser currentUser)
    : IInteractor<UpdateProfileCommand, Result>
{
    public async Task<Result> ExecuteAsync(UpdateProfileCommand command, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(actorId))
            return new ValidationError("Actor", "An authenticated user is required.");

        var profileResult = UserProfile.Create(
            command.UserId,
            command.FirstName,
            command.LastName,
            bio: command.Bio,
            updatedBy: actorId);

        if (profileResult.IsFailure)
            return profileResult.Error;

        await profiles.UpsertAsync(profileResult.Value, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
