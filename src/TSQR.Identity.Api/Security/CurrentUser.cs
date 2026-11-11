using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TSQR.Identity.Application.Abstractions;

namespace TSQR.Identity.Api.Security;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserId
    {
        get
        {
            var user = accessor.HttpContext?.User;
            var raw = user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

            return string.IsNullOrWhiteSpace(raw) ? null : raw;
        }
    }
}
