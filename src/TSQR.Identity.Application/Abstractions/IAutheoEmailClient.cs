using TSQR.Common.Results;

namespace TSQR.Identity.Application.Abstractions;

/// <summary>
/// Updates the login email owned by the autheo authentication service. The
/// caller's access token is forwarded so autheo can resolve the same user.
/// </summary>
public interface IAutheoEmailClient
{
    Task<Result> ChangeEmailAsync(
        string accessToken,
        string newEmail,
        CancellationToken ct = default);
}
