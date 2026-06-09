using System.ComponentModel.DataAnnotations;

namespace TSQR.Identity.Api.Dtos;

public record UserResponse(string Id, string? Email, string? FirstName, string? LastName, string? UserName, DateTime CreatedAt);

public record UpdateUserRequest(
    [StringLength(100)] string? FirstName,
    [StringLength(100)] string? LastName,
    [EmailAddress] string? Email);
