using TSQR.Identity.Application.Abstractions;

namespace TSQR.Identity.Tests;

internal sealed class TestCurrentUser(string? userId = "admin-1") : ICurrentUser
{
    public string? UserId { get; set; } = userId;
}
