using TSQR.Common.WebApi;
using TSQR.Identity.Api.Security;
using TSQR.Identity.Application;
using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Infrastructure;
using TSQR.Identity.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddTsqrApiDefaults(options =>
{
    options.ServiceName = "tsqr-identity";
    options.ConnectionStringName = "IdentityDb";
});

var connectionString = builder.Configuration.GetConnectionString("IdentityDb")
    ?? throw new InvalidOperationException("Connection string 'IdentityDb' not found.");

var autheoOptions = builder.Configuration.GetSection("Autheo").Get<AutheoOptions>()
    ?? new AutheoOptions();
var emailOptions = builder.Configuration.GetSection("Email").Get<EmailOptions>()
    ?? new EmailOptions();

builder.Services.AddIdentityInfrastructure(connectionString, autheoOptions, emailOptions);
builder.Services.AddApplicationFeatures();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var app = builder.Build();

app.UseTsqrApiDefaults();
app.Run();
