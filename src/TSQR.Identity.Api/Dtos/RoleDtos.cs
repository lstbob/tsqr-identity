using System.ComponentModel.DataAnnotations;

namespace TSQR.Identity.Api.Dtos;

public record RoleResponse(string Id, string Name);

public record CreateRoleRequest(
    [Required][StringLength(100)] string Name);

public record AssignRoleRequest(
    [Required] string UserId,
    [Required][StringLength(100)] string RoleName);

public record LockUserRequest(DateTime? LockoutEnd);
