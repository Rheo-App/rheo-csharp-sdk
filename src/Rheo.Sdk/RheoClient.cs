using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rheo.Sdk.Resources;

namespace Rheo.Sdk;

public sealed class RheoClient : IDisposable
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = BuildJsonOptions();

    public ItemsResource Items { get; }
    public WebhooksResource Webhooks { get; }

    public RheoClient(RheoClientOptions options)
    {
        var handler = new RetryHandler(options.MaxRetries)
        {
            InnerHandler = new HttpClientHandler(),
        };

        _http = new HttpClient(handler)
        {
            BaseAddress = options.BaseUrl,
            Timeout = options.Timeout,
        };

        _http.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        if (options.PartnerAccount is not null)
            _http.DefaultRequestHeaders.Add("x-partner-account", options.PartnerAccount);

        Items = new ItemsResource(_http, JsonOptions);
        Webhooks = new WebhooksResource(options.WebhookSecret, JsonOptions);
    }

    /// <summary>
    /// Returns an account-scoped view that routes every <c>Items</c> call to the
    /// given reseller member by setting the <c>x-partner-account</c> header per
    /// request. <paramref name="memberExternalId"/> is the reseller's own reference
    /// for the member (from <c>reseller_memberships</c> / the business "Managed
    /// accounts" UI). Lets one client + API key serve many managed accounts without
    /// constructing a client per account, and overrides any client-level
    /// <see cref="RheoClientOptions.PartnerAccount"/> for calls made through it.
    /// </summary>
    public RheoAccountScope ForAccount(string memberExternalId) =>
        new(_http, JsonOptions, memberExternalId);

    public void Dispose() => _http.Dispose();

    private static JsonSerializerOptions BuildJsonOptions() =>
        new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(SnakeCaseNamingPolicy.Instance) },
        };
}
