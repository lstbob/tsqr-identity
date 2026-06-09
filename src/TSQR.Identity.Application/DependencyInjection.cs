using Microsoft.Extensions.DependencyInjection;
using TSQR.Identity.Application.Commands.Auth;
using TSQR.Identity.Application.Commands.Roles;
using TSQR.Identity.Application.Commands.Users;
using TSQR.Identity.Application.Queries.Roles;
using TSQR.Identity.Application.Queries.Users;

namespace TSQR.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthFeatures(this IServiceCollection services)
    {
        services.AddScoped<IInteractor<RegisterCommand, Result>, RegisterCommandHandler>();
        services.AddScoped<IInteractor<LoginCommand, Result<LoginResult>>, LoginCommandHandler>();
        services.AddScoped<IInteractor<RefreshTokenCommand, Result<LoginResult>>, RefreshTokenCommandHandler>();
        services.AddScoped<IInteractor<LogoutCommand, Result>, LogoutCommandHandler>();
        services.AddScoped<IInteractor<ChangePasswordCommand, Result>, ChangePasswordCommandHandler>();
        services.AddScoped<IInteractor<GetCurrentUserQuery, Result<CurrentUserResult>>, GetCurrentUserQueryHandler>();
        return services;
    }

    public static IServiceCollection AddUserFeatures(this IServiceCollection services)
    {
        services.AddScoped<IInteractor<UpdateUserCommand, Result>, UpdateUserCommandHandler>();
        services.AddScoped<IInteractor<DeleteUserCommand, Result>, DeleteUserCommandHandler>();
        services.AddScoped<IInteractor<LockUserCommand, Result>, LockUserCommandHandler>();
        services.AddScoped<IInteractor<GetUsersQuery, Result<List<UserListItem>>>, GetUsersQueryHandler>();
        services.AddScoped<IInteractor<GetUserQuery, Result<UserListItem>>, GetUserQueryHandler>();
        return services;
    }

    public static IServiceCollection AddRoleFeatures(this IServiceCollection services)
    {
        services.AddScoped<IInteractor<CreateRoleCommand, Result>, CreateRoleCommandHandler>();
        services.AddScoped<IInteractor<AssignRoleCommand, Result>, AssignRoleCommandHandler>();
        services.AddScoped<IInteractor<RemoveRoleCommand, Result>, RemoveRoleCommandHandler>();
        services.AddScoped<IInteractor<GetRolesQuery, Result<List<RoleListItem>>>, GetRolesQueryHandler>();
        return services;
    }

    public static IServiceCollection AddApplicationFeatures(this IServiceCollection services)
    {
        services.AddAuthFeatures();
        services.AddUserFeatures();
        services.AddRoleFeatures();
        return services;
    }
}
