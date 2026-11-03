using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TSQR.Common.Errors;
using TSQR.Common.Results;
using TSQR.Identity.Application.Abstractions;
using TSQR.Identity.Infrastructure.Options;

namespace TSQR.Identity.Infrastructure.Clients;

/// <summary>
/// Calls autheo's internal change-email endpoint. The caller's bearer token is
/// forwarded so autheo applies the change to the same user, and the shared
/// internal API key proves the call came from identity.
/// </summary>
public sealed class AutheoEmailClient(
    HttpClient http,
    AutheoOptions options,
    ILogger<AutheoEmailClient> logger) : IAutheoEmailClient
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<Result> ChangeEmailAsync(
        string accessToken,
        string newEmail,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/change-email")
        {
            Content = JsonContent.Create(new { newEmail }, options: JsonOptions),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (!string.IsNullOrWhiteSpace(options.InternalApiKey))
            request.Headers.Add("X-Internal-Api-Key", options.InternalApiKey);

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Timed out calling autheo change-email.");
            return new DomainError(
                "AutheoTimeout",
                "The authentication service timed out. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Could not reach autheo change-email.");
            return new DomainError(
                "AutheoUnavailable",
                "The authentication service is unavailable. Please try again.");
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
                return Result.Success();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new ValidationError(
                    "SessionExpired",
                    "Your session is no longer valid. Please sign in again.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError(
                    "Autheo rejected the internal change-email call. Check Autheo:InternalApiKey.");
                return new DomainError(
                    "AutheoForbidden",
                    "The authentication service rejected the email change.");
            }

            var error = await TryReadErrorAsync(response, ct);
            if (error is not null && !string.IsNullOrWhiteSpace(error.Message))
            {
                return new ValidationError(
                    string.IsNullOrWhiteSpace(error.Code) ? "EmailChangeFailed" : error.Code,
                    error.Message);
            }

            return new DomainError(
                "EmailChangeFailed",
                $"The authentication service rejected the email change ({(int)response.StatusCode}).");
        }
    }

    private static async Task<AutheoError?> TryReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<AutheoError>(JsonOptions, ct);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private sealed record AutheoError(string? Code, string? Message);
}
