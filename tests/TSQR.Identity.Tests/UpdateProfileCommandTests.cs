using TSQR.Common.Errors;
using TSQR.Identity.Application.Commands.Profile;
using TSQR.Identity.Domain;
using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Tests;

public class UpdateProfileCommandTests
{
    [Fact]
    public async Task Execute_UpsertsProfileWithoutRead()
    {
        var profiles = new RecordingProfileRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new UpdateProfileCommandHandler(profiles, uow);

        var result = await handler.ExecuteAsync(
            new UpdateProfileCommand("user-1", "Ada", "Lovelace", "Mathematician"));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, profiles.GetByIdCalls);
        Assert.Equal(1, profiles.UpsertCalls);
        Assert.Equal(1, uow.SaveCalls);

        var profile = Assert.IsType<UserProfile>(profiles.Upserted);
        Assert.Equal("user-1", profile.Id);
        Assert.Equal("Ada", profile.FirstName);
        Assert.Equal("Lovelace", profile.LastName);
        Assert.Equal("Mathematician", profile.Bio);
    }

    [Fact]
    public async Task Execute_BlankUserId_ReturnsValidationErrorWithoutWriting()
    {
        var profiles = new RecordingProfileRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new UpdateProfileCommandHandler(profiles, uow);

        var result = await handler.ExecuteAsync(
            new UpdateProfileCommand(" ", "Ada", "Lovelace", null));

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(0, profiles.UpsertCalls);
        Assert.Equal(0, uow.SaveCalls);
    }

    private sealed class RecordingProfileRepository : IProfileRepository
    {
        public int GetByIdCalls { get; private set; }
        public int UpsertCalls { get; private set; }
        public UserProfile? Upserted { get; private set; }

        public Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default)
        {
            GetByIdCalls++;
            return Task.FromResult<UserProfile?>(null);
        }

        public Task<bool> ExistsAsync(string userId, CancellationToken ct = default) =>
            Task.FromResult(false);

        public Task CreateAsync(UserProfile profile, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task UpsertAsync(UserProfile profile, CancellationToken ct = default)
        {
            UpsertCalls++;
            Upserted = profile;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserProfile profile, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int SaveCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.FromResult(0);
        }
    }
}
