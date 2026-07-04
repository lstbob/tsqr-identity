using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Application.Commands.Profile;

public record UpdateProfileCommand(string UserId, string? FirstName, string? LastName, string? Bio);

public class UpdateProfileCommandHandler(IProfileRepository profiles, IUnitOfWork uow)
    : IInteractor<UpdateProfileCommand, Result>
{
    public async Task<Result> ExecuteAsync(UpdateProfileCommand command, CancellationToken ct = default)
    {
        var profile = await profiles.GetByIdAsync(command.UserId, ct);
        if (profile is null)
        {
            profile = new UserProfile
            {
                Id = command.UserId,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Bio = command.Bio,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await profiles.CreateAsync(profile, ct);
        }
        else
        {
            if (command.FirstName is not null) profile.FirstName = command.FirstName;
            if (command.LastName is not null) profile.LastName = command.LastName;
            if (command.Bio is not null) profile.Bio = command.Bio;
            profile.UpdatedAt = DateTime.UtcNow;
            await profiles.UpdateAsync(profile, ct);
        }

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
