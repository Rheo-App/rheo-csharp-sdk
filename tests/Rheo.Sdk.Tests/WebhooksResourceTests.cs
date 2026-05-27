using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Rheo.Sdk;
using Rheo.Sdk.Exceptions;
using Rheo.Sdk.Types;
using Xunit;

namespace Rheo.Sdk.Tests;

public sealed class WebhooksResourceTests
{
    private const string TestSecret = "whsec_testkey1234567890abcdefghijklmnop";

    private static RheoClient BuildClient() =>
        new(new RheoClientOptions
        {
            ApiKey = "test_key",
            WebhookSecret = TestSecret,
        });

    private static string Sign(string payload)
    {
        var key = Encoding.UTF8.GetBytes(TestSecret);
        var hash = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(payload));
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    [Fact]
    public void Verify_ValidItemSoldPayload_ReturnsParsedEvent()
    {
        var payload = """
            {
              "id": "evt_01",
              "timestamp": "2026-05-27T10:00:00Z",
              "apiVersion": "v1",
              "type": "item.sold",
              "externalId": "PART-001",
              "salePrice": 950.0,
              "platform": "tradera"
            }
            """;

        using var client = BuildClient();
        var evt = client.Webhooks.Verify(payload, Sign(payload));

        var sold = Assert.IsType<ItemSoldEvent>(evt);
        Assert.Equal("PART-001", sold.ExternalId);
        Assert.Equal(950.0, sold.SalePrice);
    }

    [Fact]
    public void Verify_TamperedPayload_Throws()
    {
        var payload = """{"id":"evt_02","timestamp":"2026-05-27T10:00:00Z","apiVersion":"v1","type":"item.created","externalId":"X"}""";
        var sig = Sign(payload);
        var tampered = payload.Replace("item.created", "item.sold");

        using var client = BuildClient();
        Assert.Throws<RheoWebhookSignatureException>(() => client.Webhooks.Verify(tampered, sig));
    }

    [Fact]
    public void Verify_WithoutSecret_Throws()
    {
        using var client = new RheoClient(new RheoClientOptions { ApiKey = "key" });
        Assert.Throws<RheoWebhookSignatureException>(() =>
            client.Webhooks.Verify("payload", "sha256=abc"));
    }
}
