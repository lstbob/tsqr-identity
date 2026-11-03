using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Domain.Repositories;
using TSQR.Identity.Infrastructure.Clients;
using TSQR.Identity.Infrastructure.Dapper;
using TSQR.Identity.Infrastructure.Options;
using TSQR.Identity.Infrastructure.Repositories;
using TSQR.Identity.Infrastructure.Services;

namespace TSQR.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        string connectionString,
        AutheoOptions autheoOptions,
        EmailOptions emailOptions)
    {
        if (string.IsNullOrWhiteSpace(autheoOptions.BaseUrl))
            throw new InvalidOperationException("Autheo:BaseUrl must be configured.");

        services.AddScoped<ISqlUnitOfWork>(_ => new DapperUnitOfWork(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ISqlUnitOfWork>());

        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IUserAdminRepository, UserAdminRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPendingEmailChangeRepository, PendingEmailChangeRepository>();

        services.AddSingleton(autheoOptions);
        services.AddHttpClient<IAutheoEmailClient, AutheoEmailClient>(client =>
        {
            client.BaseAddress = new Uri(autheoOptions.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddSingleton(emailOptions);
        if (!string.IsNullOrWhiteSpace(emailOptions.SmtpHost))
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        else
            services.AddScoped<IEmailSender, LoggingEmailSender>();

        return services;
    }
}
