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

    public RheoEvent Verify(ReadOnlySpan<byte> payload, string signature)
    {
        if (_secret is null)
            throw new RheoWebhookSignatureException(
                "WebhookSecret must be provided to RheoClient to verify webhook signatures");

        var sig = signature.StartsWith("sha256=", StringComparison.Ordinal)
            ? signature[7..]
            : signature;

        var expected = ComputeHmac(payload, _secret);

        if (!CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(sig),
            Convert.FromHexString(expected)))
        {
            throw new RheoWebhookSignatureException();
        }

        return JsonSerializer.Deserialize<RheoEvent>(payload, _json)
            ?? throw new RheoWebhookSignatureException("Payload is not a valid Rheo event");
    }

    public RheoEvent Verify(string payload, string signature) =>
        Verify(Encoding.UTF8.GetBytes(payload), signature);

    public RheoEvent ParseEvent(string json) =>
        JsonSerializer.Deserialize<RheoEvent>(json, _json)
            ?? throw new JsonException("Payload is not a valid Rheo event");

    private static string ComputeHmac(ReadOnlySpan<byte> payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var hash = HMACSHA256.HashData(key, payload);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
