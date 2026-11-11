using TSQR.Common.Errors;
using TSQR.Identity.Application.Commands.Users;
using TSQR.Identity.Domain;
using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Tests;

public class UserProfileCommandTests
{
    [Fact]
    public async Task UpdateUser_ValidNames_UpdatesUserAndAuditFields()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new UpdateUserCommandHandler(users, uow, new TestCurrentUser("admin-1"));

        var result = await handler.ExecuteAsync(new UpdateUserCommand("user-1", "Grace", "Hopper"));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, users.UpdateCalls);
        Assert.Equal(1, uow.SaveCalls);
        Assert.Equal("Grace", users.User!.FirstName);
        Assert.Equal("Hopper", users.User.LastName);
        Assert.Equal("admin-1", users.User.UpdatedBy);
    }

    [Fact]
    public async Task UpdateUser_BlankFirstName_ReturnsValidationErrorWithoutWriting()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new UpdateUserCommandHandler(users, uow, new TestCurrentUser());

        var result = await handler.ExecuteAsync(new UpdateUserCommand("user-1", " ", "Hopper"));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("FirstName", error.Code);
        Assert.Equal(0, users.UpdateCalls);
        Assert.Equal(0, uow.SaveCalls);
    }

    [Fact]
    public async Task UpdateUser_MissingActor_ReturnsValidationErrorWithoutWriting()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new UpdateUserCommandHandler(users, uow, new TestCurrentUser(null));

        var result = await handler.ExecuteAsync(new UpdateUserCommand("user-1", "Grace", "Hopper"));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("Actor", error.Code);
        Assert.Equal(0, users.UpdateCalls);
        Assert.Equal(0, uow.SaveCalls);
    }

    [Fact]
    public async Task SuspendUser_FutureDate_SuspendsUserWithAuditFields()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new SuspendUserCommandHandler(users, uow, new TestCurrentUser("admin-1"));
        var until = DateTime.UtcNow.AddHours(1);

        var result = await handler.ExecuteAsync(
            new SuspendUserCommand("user-1", until, "Repeated abuse"));

        Assert.True(result.IsSuccess);
        Assert.Equal(until, users.User!.LockoutEnd);
        Assert.Equal("Repeated abuse", users.User.LockReason);
        Assert.Equal("admin-1", users.User.LockedBy);
        Assert.Equal("admin-1", users.User.UpdatedBy);
        Assert.Equal(1, users.UpdateCalls);
        Assert.Equal(1, uow.SaveCalls);
    }

    [Fact]
    public async Task SuspendUser_PastDate_ReturnsValidationErrorWithoutWriting()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new SuspendUserCommandHandler(users, uow, new TestCurrentUser());

        var result = await handler.ExecuteAsync(
            new SuspendUserCommand("user-1", DateTime.UtcNow.AddMinutes(-1), "Abuse"));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("LockoutEnd", error.Code);
        Assert.Null(users.User!.LockoutEnd);
        Assert.Equal(0, users.UpdateCalls);
        Assert.Equal(0, uow.SaveCalls);
    }

    [Fact]
    public async Task SuspendUser_MissingActor_ReturnsValidationErrorWithoutWriting()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new SuspendUserCommandHandler(users, uow, new TestCurrentUser(null));

        var result = await handler.ExecuteAsync(
            new SuspendUserCommand("user-1", DateTime.UtcNow.AddHours(1), "Abuse"));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("Actor", error.Code);
        Assert.Equal(0, users.UpdateCalls);
        Assert.Equal(0, uow.SaveCalls);
    }

    [Fact]
    public async Task ReinstateUser_SuspendedUser_ClearsLockoutAndRecordsActor()
    {
        var users = new RecordingUserAdminRepository();
        Assert.True(users.User!.Suspend(DateTime.UtcNow.AddHours(1), "Abuse", "admin-1").IsSuccess);
        var uow = new RecordingUnitOfWork();
        var handler = new ReinstateUserCommandHandler(users, uow, new TestCurrentUser("admin-2"));

        var result = await handler.ExecuteAsync(new ReinstateUserCommand("user-1"));

        Assert.True(result.IsSuccess);
        Assert.Null(users.User.LockoutEnd);
        Assert.Null(users.User.LockedBy);
        Assert.Equal("admin-2", users.User.UpdatedBy);
        Assert.Equal(1, users.UpdateCalls);
        Assert.Equal(1, uow.SaveCalls);
    }

    [Fact]
    public async Task ReinstateUser_NotSuspended_ReturnsDomainErrorWithoutWriting()
    {
        var users = new RecordingUserAdminRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new ReinstateUserCommandHandler(users, uow, new TestCurrentUser());

        var result = await handler.ExecuteAsync(new ReinstateUserCommand("user-1"));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("LockoutEnd", error.Code);
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
                bio: "Mathematician",
                createdBy: "creator-1");
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
