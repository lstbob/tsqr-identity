using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TSQR.Identity.Api.Dtos;
using TSQR.Identity.Application;
using TSQR.Identity.Infrastructure;
using TSQR.Identity.Infrastructure.Dapper;

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

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "tsqr-autheo";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "tsqr-services";

builder.Services.AddIdentityInfrastructure(connectionString);
builder.Services.AddApplicationFeatures();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
