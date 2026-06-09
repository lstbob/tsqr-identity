using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TSQR.Identity.Api.Dtos;
using TSQR.Identity.Application;
using TSQR.Identity.Infrastructure;
using TSQR.Identity.Infrastructure.Data;
using TSQR.Identity.Infrastructure.Seed;
using TSQR.Identity.Infrastructure.Services;

TypeHandlerRegistrations.EnsureRegistered();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var firstError = ctx.ModelState
                .FirstOrDefault(kvp => kvp.Value?.Errors.Count > 0);

            var error = firstError.Value?.Errors.FirstOrDefault();
            var code = firstError.Key;
            var message = error?.ErrorMessage ?? "Validation failed.";

            return new BadRequestObjectResult(new ErrorResponse(code, message));
        };
    });

var connectionString = builder.Configuration.GetConnectionString("IdentityDb")
    ?? throw new InvalidOperationException("Connection string 'IdentityDb' not found.");

builder.Services.AddIdentityInfrastructure(connectionString);
builder.Services.AddApplicationFeatures();

var jwtSection = builder.Configuration.GetSection(JwtOptions.Section);
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration not found.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await IdentitySeeder.SeedAsync(app.Services);

app.Run();
