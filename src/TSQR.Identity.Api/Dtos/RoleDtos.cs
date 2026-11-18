using System.ComponentModel.DataAnnotations;

namespace TSQR.Identity.Api.Dtos;

public record RoleResponse(string Id, string Name, IReadOnlyCollection<string> Permissions);

public record CreateRoleRequest(
    [Required][StringLength(100)] string Name,
    IReadOnlyCollection<string>? Permissions = null);

public record AssignRoleRequest(
    [Required] string UserId,
    [Required][StringLength(100)] string RoleName);
