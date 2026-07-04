using System.Security.Cryptography;
using System.Text;

namespace TSQR.Identity.Application.Commands.Profile;

public record RequestEmailChangeCommand(string UserId, string NewEmail);

public class RequestEmailChangeCommandHandler : IInteractor<RequestEmailChangeCommand, Result<string>>
{
    public async Task<Result<string>> ExecuteAsync(RequestEmailChangeCommand command, CancellationToken ct = default)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return Result<string>.Success(token);
    }
}
