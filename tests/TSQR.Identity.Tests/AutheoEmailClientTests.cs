using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging.Abstractions;
using TSQR.Common.Errors;
using TSQR.Identity.Infrastructure.Clients;
using TSQR.Identity.Infrastructure.Options;

namespace TSQR.Identity.Tests;

public class AutheoEmailClientTests
{
    [Fact]
    public async Task ChangeEmail_ExchangesIdentityTokenBeforeCallingAutheo()
    {
        var handler = new StubHandler(request =>
            request.RequestUri?.AbsolutePath switch
            {
                "/internal/auth/exchange" => Json(new
                {
                    accessToken = "autheo-token",
                    expiresAt = DateTime.UtcNow.AddMinutes(5),
                }),
                "/api/auth/change-email" => new HttpResponseMessage(HttpStatusCode.OK),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound),
            });
        var client = CreateClient(handler, "internal-key");

        var result = await client.ChangeEmailAsync(
            "identity-token",
            "new@example.com");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, handler.Requests.Count);

        var exchange = handler.Requests[0];
        Assert.Equal("/internal/auth/exchange", exchange.Path);
        Assert.Equal("Bearer identity-token", exchange.Authorization);
        Assert.Equal("internal-key", exchange.InternalApiKey);
        Assert.Contains("\"audience\":\"tsqr-autheo\"", exchange.Body);

        var change = handler.Requests[1];
        Assert.Equal("/api/auth/change-email", change.Path);
        Assert.Equal("Bearer autheo-token", change.Authorization);
        Assert.Equal("internal-key", change.InternalApiKey);
    }

    [Fact]
    public async Task ChangeEmail_WhenExchangeIsUnauthorized_ReturnsSessionExpired()
    {
        var handler = new StubHandler(request =>
            request.RequestUri?.AbsolutePath == "/internal/auth/exchange"
                ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                : new HttpResponseMessage(HttpStatusCode.OK));
        var client = CreateClient(handler, "internal-key");

        var result = await client.ChangeEmailAsync("identity-token", "new@example.com");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("SessionExpired", error.Code);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task ChangeEmail_WithoutInternalApiKey_FailsBeforeCallingAutheo()
    {
        var handler = new StubHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK));
        var client = CreateClient(handler, internalApiKey: string.Empty);

        var result = await client.ChangeEmailAsync("identity-token", "new@example.com");

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("AutheoForbidden", error.Code);
        Assert.Empty(handler.Requests);
    }

    private static AutheoEmailClient CreateClient(
        StubHandler handler,
        string internalApiKey)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://autheo/"),
        };
        return new AutheoEmailClient(
            http,
            new AutheoOptions
            {
                BaseUrl = "http://autheo",
                InternalApiKey = internalApiKey,
            },
            NullLogger<AutheoEmailClient>.Instance);
    }

    private static HttpResponseMessage Json(object value) =>
        new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };

    private sealed record CapturedRequest(
        string Path,
        string? Authorization,
        string? InternalApiKey,
        string Body);

    private sealed class StubHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<CapturedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            var internalApiKey = request.Headers.TryGetValues(
                "X-Internal-Api-Key",
                out var values)
                ? values.Single()
                : null;
            Requests.Add(new CapturedRequest(
                request.RequestUri?.AbsolutePath ?? string.Empty,
                request.Headers.Authorization?.ToString(),
                internalApiKey,
                body));
            return responder(request);
        }
    }
}
