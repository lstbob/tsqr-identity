using System.ComponentModel.DataAnnotations;

namespace TSQR.Identity.Api.Dtos;

public record UpdateProfileRequest(
    [StringLength(100)] string? FirstName,
    [StringLength(100)] string? LastName,
    [StringLength(500)] string? Bio);

public record EmailChangeRequest(
    [Required][EmailAddress] string NewEmail);

public record ConfirmEmailChangeRequest(
    [Required][EmailAddress] string NewEmail,
    [Required] string Token);
