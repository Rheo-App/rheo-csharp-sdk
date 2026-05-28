using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Rheo.Sdk.Exceptions;
using Rheo.Sdk.Types;

namespace Rheo.Sdk.Resources;

public sealed class WebhooksResource
{
    private readonly string? _secret;
    private readonly JsonSerializerOptions _json;

    internal WebhooksResource(string? secret, JsonSerializerOptions json)
    {
        _secret = secret;
        _json = json;
    }

    /// <summary>
    /// Verify a webhook signature and return the parsed event. With multiple webhook
    /// endpoints each endpoint has its OWN signing secret — verify with the secret of
    /// the endpoint that received the event. Pass one or more <paramref name="secrets"/>
    /// to override the client-level secret (e.g. the receiving endpoint's secret, or a
    /// set of candidate secrets during rotation); any match succeeds. With no
    /// <paramref name="secrets"/>, the client-level <c>WebhookSecret</c> is used.
    /// </summary>
    public RheoEvent Verify(ReadOnlySpan<byte> payload, string signature, params string[] secrets)
    {
        var candidates = ResolveSecrets(secrets);

        var sig = signature.StartsWith("sha256=", StringComparison.Ordinal)
            ? signature[7..]
            : signature;
        var provided = Convert.FromHexString(sig);

        var matched = false;
        foreach (var secret in candidates)
        {
            var expected = Convert.FromHexString(ComputeHmac(payload, secret));
            if (CryptographicOperations.FixedTimeEquals(provided, expected))
            {
                matched = true;
                break;
            }
        }

        if (!matched)
            throw new RheoWebhookSignatureException();

        return JsonSerializer.Deserialize<RheoEvent>(payload, _json)
            ?? throw new RheoWebhookSignatureException("Payload is not a valid Rheo event");
    }

    public RheoEvent Verify(string payload, string signature, params string[] secrets) =>
        Verify(Encoding.UTF8.GetBytes(payload), signature, secrets);

    public RheoEvent ParseEvent(string json) =>
        JsonSerializer.Deserialize<RheoEvent>(json, _json)
            ?? throw new JsonException("Payload is not a valid Rheo event");

    private string[] ResolveSecrets(string[] overrides)
    {
        var source = overrides is { Length: > 0 }
            ? overrides
            : _secret is not null
                ? new[] { _secret }
                : Array.Empty<string>();

        var list = source.Where(s => !string.IsNullOrEmpty(s)).ToArray();
        if (list.Length == 0)
            throw new RheoWebhookSignatureException(
                "A webhook secret must be provided (RheoClient WebhookSecret or the secret argument) to verify webhook signatures");
        return list;
    }

    private static string ComputeHmac(ReadOnlySpan<byte> payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var hash = HMACSHA256.HashData(key, payload);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
