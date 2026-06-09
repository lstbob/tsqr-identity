using System.ComponentModel.DataAnnotations;

namespace TSQR.Identity.Api.Dtos;

public record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][StringLength(100, MinimumLength = 8)] string Password,
    [Required][StringLength(100)] string FirstName,
    [Required][StringLength(100)] string LastName);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password);

public record TokenResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken);

public record RefreshTokenRequest(
    [Required] string RefreshToken);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required][StringLength(100, MinimumLength = 8)] string NewPassword);
