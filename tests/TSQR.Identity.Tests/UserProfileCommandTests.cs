using TSQR.Common.Errors;
using TSQR.Identity.Application.Commands.Users;
using TSQR.Identity.Domain;
using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Tests;

public class UserProfileCommandTests
{
    [Fact]
    public async Task UpdateUser_ValidNames_UpdatesUser()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new UpdateUserCommandHandler(users, uow);

        var result = await handler.ExecuteAsync(new UpdateUserCommand("user-1", "Grace", "Hopper"));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, users.UpdateCalls);
        Assert.Equal(1, uow.SaveCalls);
        Assert.Equal("Grace", users.User!.FirstName);
        Assert.Equal("Hopper", users.User.LastName);
    }

    [Fact]
    public async Task UpdateUser_BlankFirstName_ReturnsValidationErrorWithoutWriting()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new UpdateUserCommandHandler(users, uow);

        var result = await handler.ExecuteAsync(new UpdateUserCommand("user-1", " ", "Hopper"));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("FirstName", error.Code);
        Assert.Equal(0, users.UpdateCalls);
        Assert.Equal(0, uow.SaveCalls);
    }

    [Fact]
    public async Task LockUser_FutureDate_LocksUser()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new LockUserCommandHandler(users, uow);
        var lockoutEnd = DateTime.UtcNow.AddHours(1);

        var result = await handler.ExecuteAsync(new LockUserCommand("user-1", lockoutEnd));

        Assert.True(result.IsSuccess);
        Assert.Equal(lockoutEnd, users.User!.LockoutEnd);
        Assert.Equal(1, users.UpdateCalls);
        Assert.Equal(1, uow.SaveCalls);
    }

    [Fact]
    public async Task LockUser_PastDate_ReturnsValidationErrorWithoutWriting()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new LockUserCommandHandler(users, uow);

        var result = await handler.ExecuteAsync(
            new LockUserCommand("user-1", DateTime.UtcNow.AddMinutes(-1)));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("LockoutEnd", error.Code);
        Assert.Null(users.User!.LockoutEnd);
        Assert.Equal(0, users.UpdateCalls);
        Assert.Equal(0, uow.SaveCalls);
    }

    private sealed class RecordingUserAdminRepository : IUserAdminRepository
    {
        public UserProfile? User { get; } = CreateUser();
        public int UpdateCalls { get; private set; }

        public Task<List<UserProfile>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult(new List<UserProfile> { User! });

        public Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default) =>
            Task.FromResult(User);

        public Task UpdateAsync(UserProfile user, CancellationToken ct = default)
        {
            UpdateCalls++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string userId, CancellationToken ct = default) =>
            Task.CompletedTask;

        private static UserProfile CreateUser()
        {
            var result = UserProfile.Create(
                "user-1",
                "Ada",
                "Lovelace",
                bio: "Mathematician");
            Assert.True(result.IsSuccess);
            return result.Value;
        }
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
