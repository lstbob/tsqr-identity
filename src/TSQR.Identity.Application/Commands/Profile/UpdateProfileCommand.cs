using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Profile;

public record UpdateProfileCommand(string UserId, string? FirstName, string? LastName, string? Bio);

public class UpdateProfileCommandHandler(IProfileRepository profiles, IUnitOfWork uow)
    : IInteractor<UpdateProfileCommand, Result>
{
    public async Task<Result> ExecuteAsync(UpdateProfileCommand command, CancellationToken ct = default)
    {
        var profileResult = UserProfile.Create(
            command.UserId,
            command.FirstName,
            command.LastName,
            bio: command.Bio);

        if (profileResult.IsFailure)
            return profileResult.Error;

        await profiles.UpsertAsync(profileResult.Value, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
