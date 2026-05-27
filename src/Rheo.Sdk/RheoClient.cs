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

    public void Dispose() => _http.Dispose();

    private static JsonSerializerOptions BuildJsonOptions() =>
        new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(SnakeCaseNamingPolicy.Instance) },
        };
}
