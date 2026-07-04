using System.ComponentModel.DataAnnotations;

namespace TSQR.Identity.Api.Dtos;

public record UserResponse(string Id, string? FirstName, string? LastName, DateTime CreatedAt);

public record CurrentUserResponse(
    string Id,
    string? FirstName,
    string? LastName,
    IList<string> Roles,
    DateTime CreatedAt);

public record UpdateUserRequest(
    [StringLength(100)] string? FirstName,
    [StringLength(100)] string? LastName);
