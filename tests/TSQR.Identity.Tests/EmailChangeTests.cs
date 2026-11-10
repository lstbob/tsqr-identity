using TSQR.Common.Errors;
using TSQR.Common.Results;
using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Application.Commands.Profile;
using TSQR.Identity.Application.Security;
using TSQR.Identity.Domain;
using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Tests;

public class EmailChangeTests
{
    [Fact]
    public async Task RequestEmailChange_PersistsHashedTokenAndDispatchesRawToken()
    {
        var pendingRepo = new FakePendingEmailChangeRepository();
        var profiles = new FakeProfileRepository();
        var email = new FakeEmailSender();
        var uow = new FakeUnitOfWork();
        var handler = new RequestEmailChangeCommandHandler(pendingRepo, profiles, email, uow);

        var result = await handler.ExecuteAsync(
            new RequestEmailChangeCommand("user-1", " New@Example.COM "));

        Assert.True(result.IsSuccess);
        var pending = Assert.Single(pendingRepo.Items);
        Assert.Equal("new@example.com", pending.NewEmail);
        Assert.Equal("user-1", pending.UserId);
        Assert.NotNull(email.Token);
        Assert.NotEqual(email.Token, pending.TokenHash);
        Assert.Equal(EmailChangeToken.Hash(email.Token!), pending.TokenHash);
        Assert.True(pending.ExpiresAt > DateTime.UtcNow);
        Assert.False(pending.IsConfirmed);
        Assert.NotNull(profiles.Created);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task RequestEmailChange_ReplacesEarlierUnconfirmedRequest()
    {
        var pendingRepo = new FakePendingEmailChangeRepository();
        var old = PendingEmailChange.Create(
            "user-1",
            "old@example.com",
            EmailChangeToken.Hash("old-token"),
            DateTime.UtcNow.AddHours(2));
        pendingRepo.Items.Add(old);

        var handler = new RequestEmailChangeCommandHandler(
            pendingRepo,
            new FakeProfileRepository { Exists = true },
            new FakeEmailSender(),
            new FakeUnitOfWork());

        var result = await handler.ExecuteAsync(
            new RequestEmailChangeCommand("user-1", "new@example.com"));

        Assert.True(result.IsSuccess);
        var pending = Assert.Single(pendingRepo.Items);
        Assert.NotEqual(old.Id, pending.Id);
        Assert.Equal("new@example.com", pending.NewEmail);
    }

    [Fact]
    public async Task RequestEmailChange_InvalidEmail_ReturnsValidationError()
    {
        var pendingRepo = new FakePendingEmailChangeRepository();
        var email = new FakeEmailSender();
        var handler = new RequestEmailChangeCommandHandler(
            pendingRepo,
            new FakeProfileRepository(),
            email,
            new FakeUnitOfWork());

        var result = await handler.ExecuteAsync(
            new RequestEmailChangeCommand("user-1", "not-an-email"));

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationError>(result.Error);
        Assert.Empty(pendingRepo.Items);
        Assert.Null(email.Token);
    }

    [Fact]
    public async Task RequestEmailChange_EmailDeliveryFailure_ReturnsError()
    {
        var pendingRepo = new FakePendingEmailChangeRepository();
        var handler = new RequestEmailChangeCommandHandler(
            pendingRepo,
            new FakeProfileRepository(),
            new FakeEmailSender { ThrowOnSend = true },
            new FakeUnitOfWork());

        var result = await handler.ExecuteAsync(
            new RequestEmailChangeCommand("user-1", "new@example.com"));

        Assert.True(result.IsFailure);
        Assert.Equal("EmailDeliveryFailed", result.Error.Code);
        Assert.Single(pendingRepo.Items);
    }

    [Fact]
    public async Task ChangeEmail_ValidToken_UpdatesAutheoAndConfirms()
    {
        var token = EmailChangeToken.Generate();
        var pendingRepo = new FakePendingEmailChangeRepository();
        var pending = AddPending(pendingRepo, token);
        var autheo = new FakeAutheoEmailClient();
        var handler = new ChangeEmailCommandHandler(
            pendingRepo, autheo, new FakeUnitOfWork());

        var result = await handler.ExecuteAsync(new ChangeEmailCommand(
            "user-1", "new@example.com", token, "access-token"));

        Assert.True(result.IsSuccess);
        Assert.True(pending.IsConfirmed);
        Assert.Equal(1, autheo.CallCount);
        Assert.Equal("access-token", autheo.AccessToken);
        Assert.Equal("new@example.com", autheo.NewEmail);
    }

    [Fact]
    public async Task ChangeEmail_ExpiredToken_DoesNotCallAutheo()
    {
        var token = EmailChangeToken.Generate();
        var pendingRepo = new FakePendingEmailChangeRepository();
        var pending = AddPending(
            pendingRepo,
            token,
            expiresAt: DateTime.UtcNow.AddMinutes(-1),
            createdAt: DateTime.UtcNow.AddHours(-2));
        var autheo = new FakeAutheoEmailClient();
        var handler = new ChangeEmailCommandHandler(
            pendingRepo, autheo, new FakeUnitOfWork());

        var result = await handler.ExecuteAsync(new ChangeEmailCommand(
            "user-1", "new@example.com", token, "access-token"));

        Assert.True(result.IsFailure);
        Assert.Equal("Token", result.Error.Code);
        Assert.False(pending.IsConfirmed);
        Assert.Equal(0, autheo.CallCount);
    }

    [Fact]
    public async Task ChangeEmail_WrongUser_RejectsToken()
    {
        var token = EmailChangeToken.Generate();
        var pendingRepo = new FakePendingEmailChangeRepository();
        AddPending(pendingRepo, token);
        var autheo = new FakeAutheoEmailClient();
        var handler = new ChangeEmailCommandHandler(
            pendingRepo, autheo, new FakeUnitOfWork());

        var result = await handler.ExecuteAsync(new ChangeEmailCommand(
            "user-2", "new@example.com", token, "access-token"));

        Assert.True(result.IsFailure);
        Assert.Equal(0, autheo.CallCount);
    }

    [Fact]
    public async Task ChangeEmail_WrongEmail_RejectsToken()
    {
        var token = EmailChangeToken.Generate();
        var pendingRepo = new FakePendingEmailChangeRepository();
        AddPending(pendingRepo, token);
        var autheo = new FakeAutheoEmailClient();
        var handler = new ChangeEmailCommandHandler(
            pendingRepo, autheo, new FakeUnitOfWork());

        var result = await handler.ExecuteAsync(new ChangeEmailCommand(
            "user-1", "different@example.com", token, "access-token"));

        Assert.True(result.IsFailure);
        Assert.Equal(0, autheo.CallCount);
    }

    [Fact]
    public async Task ChangeEmail_AutheoFailure_LeavesRequestUnconfirmed()
    {
        var token = EmailChangeToken.Generate();
        var pendingRepo = new FakePendingEmailChangeRepository();
        var pending = AddPending(pendingRepo, token);
        var autheo = new FakeAutheoEmailClient
        {
            NextResult = new ValidationError("Email", "That email address is already in use."),
        };
        var handler = new ChangeEmailCommandHandler(
            pendingRepo, autheo, new FakeUnitOfWork());

        var result = await handler.ExecuteAsync(new ChangeEmailCommand(
            "user-1", "new@example.com", token, "access-token"));

        Assert.True(result.IsFailure);
        Assert.False(pending.IsConfirmed);
        Assert.Equal(1, autheo.CallCount);
    }

    [Fact]
    public async Task ChangeEmail_AlreadyConfirmed_RejectsReplay()
    {
        var token = EmailChangeToken.Generate();
        var pendingRepo = new FakePendingEmailChangeRepository();
        var pending = AddPending(pendingRepo, token);
        pending.Confirm();
        var autheo = new FakeAutheoEmailClient();
        var handler = new ChangeEmailCommandHandler(
            pendingRepo, autheo, new FakeUnitOfWork());

        var result = await handler.ExecuteAsync(new ChangeEmailCommand(
            "user-1", "new@example.com", token, "access-token"));

        Assert.True(result.IsFailure);
        Assert.Equal(0, autheo.CallCount);
    }

    [Fact]
    public void EmailChangeToken_GenerateIsUrlSafeAndHashIsStable()
    {
        var token = EmailChangeToken.Generate();
        var hash = EmailChangeToken.Hash(token);

        Assert.DoesNotContain('+', token);
        Assert.DoesNotContain('/', token);
        Assert.DoesNotContain('=', token);
        Assert.Equal(64, hash.Length);
        Assert.Equal(hash, EmailChangeToken.Hash(token));
        Assert.NotEqual(hash, EmailChangeToken.Hash(EmailChangeToken.Generate()));
    }

    private static PendingEmailChange AddPending(
        FakePendingEmailChangeRepository repo,
        string token,
        string userId = "user-1",
        string email = "new@example.com",
        DateTime? expiresAt = null,
        DateTime? createdAt = null)
    {
        var now = createdAt ?? DateTime.UtcNow;
        var pending = PendingEmailChange.Create(
            userId,
            email,
            EmailChangeToken.Hash(token),
            expiresAt ?? DateTime.UtcNow.AddHours(1),
            now);
        repo.Items.Add(pending);
        return pending;
    }

    private sealed class FakePendingEmailChangeRepository : IPendingEmailChangeRepository
    {
        public List<PendingEmailChange> Items { get; } = [];

        public Task AddAsync(PendingEmailChange change, CancellationToken ct = default)
        {
            Items.Add(change);
            return Task.CompletedTask;
        }

        public Task<PendingEmailChange?> GetByTokenHashAsync(
            string tokenHash,
            CancellationToken ct = default) =>
            Task.FromResult(Items.FirstOrDefault(x => x.TokenHash == tokenHash));

        public Task DeleteUnconfirmedForUserAsync(
            string userId,
            CancellationToken ct = default)
        {
            Items.RemoveAll(x => x.UserId == userId && !x.IsConfirmed);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(PendingEmailChange change, CancellationToken ct = default)
        {
            var index = Items.FindIndex(x => x.Id == change.Id);
            if (index >= 0)
                Items[index] = change;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        public bool Exists { get; set; }
        public UserProfile? Created { get; private set; }

        public Task<UserProfile?> GetByIdAsync(
            string userId,
            CancellationToken ct = default) =>
            Task.FromResult(Created);

        public Task<bool> ExistsAsync(string userId, CancellationToken ct = default) =>
            Task.FromResult(Exists);

        public Task CreateAsync(UserProfile profile, CancellationToken ct = default)
        {
            Created = profile;
            Exists = true;
            return Task.CompletedTask;
        }

        public Task UpsertAsync(UserProfile profile, CancellationToken ct = default)
        {
            Created = profile;
            Exists = true;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserProfile profile, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public string? Token { get; private set; }
        public string? NewEmail { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool ThrowOnSend { get; init; }

        public Task SendEmailChangeConfirmationAsync(
            string newEmail,
            string token,
            DateTime expiresAt,
            CancellationToken ct = default)
        {
            if (ThrowOnSend)
                throw new InvalidOperationException("SMTP is not configured.");

            NewEmail = newEmail;
            Token = token;
            ExpiresAt = expiresAt;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAutheoEmailClient : IAutheoEmailClient
    {
        public int CallCount { get; private set; }
        public string? AccessToken { get; private set; }
        public string? NewEmail { get; private set; }
        public Result NextResult { get; init; } = Result.Success();

        public Task<Result> ChangeEmailAsync(
            string accessToken,
            string newEmail,
            CancellationToken ct = default)
        {
            CallCount++;
            AccessToken = accessToken;
            NewEmail = newEmail;
            return Task.FromResult(NextResult);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(0);
        }
    }
}
