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

public record SuspendUserRequest(
    DateTime Until,
    [Required][StringLength(500)] string Reason);
