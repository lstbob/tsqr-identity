namespace TSQR.Identity.Application.Abstractions;

public interface ICurrentUser
{
    string? UserId { get; }
}
