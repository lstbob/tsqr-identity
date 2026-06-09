namespace TSQR.Identity.Application.Interfaces;

public interface IJwtService
{
    (string token, DateTime expiresAt) GenerateAccessToken(AppUser user, IList<string> roles);
    string GenerateRefreshToken();
    string HashToken(string token);
}
