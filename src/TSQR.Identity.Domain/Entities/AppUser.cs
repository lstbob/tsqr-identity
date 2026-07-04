using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Domain.Entities;

public class AppUser : IdentityUser<string>, IAggregateRoot
{
    public AppUser()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
