using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TSQR.Identity.Application.Interfaces;
using TSQR.Identity.Infrastructure.IdentityStores;
using TSQR.Identity.Infrastructure.Repositories;
using TSQR.Identity.Infrastructure.Services;

namespace TSQR.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<IDatabaseConnection>(_ => new IdentityDbConnection(connectionString));

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<IUserStore<AppUser>, UserStore>();
        services.AddScoped<IRoleStore<Role>, RoleStore>();

        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddRoles<Role>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        return services;
    }
}
