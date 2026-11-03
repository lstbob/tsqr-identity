namespace TSQR.Identity.Infrastructure.Options;

public sealed class AutheoOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5068";

    /// <summary>
    /// Shared service-to-service credential for autheo's internal
    /// change-email endpoint. External callers never receive this value.
    /// </summary>
    public string InternalApiKey { get; set; } = string.Empty;
}
