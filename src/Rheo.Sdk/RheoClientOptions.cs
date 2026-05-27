namespace Rheo.Sdk;

public sealed class RheoClientOptions
{
    public required string ApiKey { get; init; }
    public string? WebhookSecret { get; init; }
    public Uri BaseUrl { get; init; } = new("https://market.rheo.se");
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; init; } = 3;
    public string? PartnerAccount { get; init; }
}
