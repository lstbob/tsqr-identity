using TSQR.Common.Errors;
using TSQR.Identity.Application.Commands.Roles;
using TSQR.Identity.Domain;
using TSQR.Identity.Domain.Entities;
using TSQR.Identity.Domain.Repositories;

namespace TSQR.Identity.Tests;

public class RoleCommandTests
{
    [Fact]
    public async Task CreateRole_ValidName_CreatesNormalizedRole()
    {
        var roles = new RecordingRoleRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new CreateRoleCommandHandler(roles, uow);

        var result = await handler.ExecuteAsync(new CreateRoleCommand("  Admin  "));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, roles.CreateCalls);
        Assert.Equal(1, uow.SaveCalls);
        var role = Assert.IsType<Role>(roles.Created);
        Assert.Equal("Admin", role.Name);
        Assert.Equal("ADMIN", role.NormalizedName);
        Assert.False(string.IsNullOrWhiteSpace(role.Id));
    }

    [Fact]
    public async Task CreateRole_BlankName_ReturnsValidationErrorWithoutWriting()
    {
        var roles = new RecordingRoleRepository();
        var uow = new RecordingUnitOfWork();
        var handler = new CreateRoleCommandHandler(roles, uow);

        var result = await handler.ExecuteAsync(new CreateRoleCommand(" "));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("Name", error.Code);
        Assert.Equal(0, roles.GetByNameCalls);
        Assert.Equal(0, roles.CreateCalls);
        Assert.Equal(0, uow.SaveCalls);
    }

    [Fact]
    public async Task CreateRole_DuplicateName_ReturnsValidationErrorWithoutWriting()
    {
        var roles = new RecordingRoleRepository { Existing = CreateRole() };
        var uow = new RecordingUnitOfWork();
        var handler = new CreateRoleCommandHandler(roles, uow);

        var result = await handler.ExecuteAsync(new CreateRoleCommand("ADMIN"));

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("RoleName", error.Code);
        Assert.Equal(1, roles.GetByNameCalls);
        Assert.Equal(0, roles.CreateCalls);
        Assert.Equal(0, uow.SaveCalls);
    }

    private static Role CreateRole()
    {
        var result = Role.Create("Admin");
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private sealed class RecordingRoleRepository : IRoleRepository
    {
        public Role? Existing { get; init; }
        public int GetByNameCalls { get; private set; }
        public int CreateCalls { get; private set; }
        public Role? Created { get; private set; }

        public Task<List<Role>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult(new List<Role>());

        public Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            GetByNameCalls++;
            return Task.FromResult(Existing);
        }

        public Task CreateAsync(Role role, CancellationToken ct = default)
        {
            CreateCalls++;
            Created = role;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string roleId, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<bool> UserHasRoleAsync(string userId, string roleName, CancellationToken ct = default) =>
            Task.FromResult(false);

        public Task AssignRoleAsync(string userId, string roleId, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task RemoveRoleAsync(string userId, string roleName, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<List<string>> GetUserRolesAsync(string userId, CancellationToken ct = default) =>
            Task.FromResult(new List<string>());
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
