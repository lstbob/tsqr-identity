using TSQR.Identity.Domain.Repositories;
using TSQR.Identity.Infrastructure.Dapper;
using TSQR.Identity.Infrastructure.Repositories;

namespace TSQR.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<ISqlUnitOfWork>(_ => new DapperUnitOfWork(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ISqlUnitOfWork>());

        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IUserAdminRepository, UserAdminRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }
}
