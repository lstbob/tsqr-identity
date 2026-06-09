using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Domain.Entities;

public class Role : IdentityRole<string>
{
    public Role()
    {
        Id = Guid.NewGuid().ToString();
    }

    public Role(string name) : this()
    {
        Name = name;
    }
}
