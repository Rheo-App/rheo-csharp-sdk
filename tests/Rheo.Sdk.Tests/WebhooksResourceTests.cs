using System.Security.Cryptography;
using System.Text;
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

    private static string Sign(string payload, string secret = TestSecret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var hash = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(payload));
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    [Fact]
    public void Verify_ValidItemSoldPayload_ReturnsParsedEvent()
    {
        var payload = """
            {
              "eventId": "evt_01",
              "timestamp": "2026-05-27T10:00:00Z",
              "eventType": "item.sold",
              "data": {
                "externalId": "PART-001",
                "rheoItemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                "platform": "tradera",
                "salePrice": 950.0,
                "currency": "SEK",
                "traderaAdId": "398271634",
                "account": { "rheoUserId": "11111111-1111-1111-1111-111111111111", "memberExternalId": "10" }
              }
            }
            """;

        using var client = BuildClient();
        var evt = client.Webhooks.Verify(payload, Sign(payload));

        var sold = Assert.IsType<ItemSoldEvent>(evt);
        Assert.Equal("item.sold", sold.EventType);
        Assert.Equal("PART-001", sold.Data.ExternalId);
        Assert.Equal(950.0, sold.Data.SalePrice);
        Assert.Equal("398271634", sold.Data.TraderaAdId);
        Assert.Equal("10", sold.Data.Account?.MemberExternalId);
    }

    [Fact]
    public void Verify_TamperedPayload_Throws()
    {
        var payload = """
            {"eventId":"evt_02","timestamp":"2026-05-27T10:00:00Z","eventType":"item.created","data":{"externalId":"X","rheoItemId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","platform":"rheo","salePrice":0,"currency":"SEK"}}
            """;
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

    [Fact]
    public void Verify_WithPerEndpointSecretArray_SucceedsOnAnyMatch()
    {
        var payload = """
            {"eventId":"evt_03","timestamp":"2026-05-27T10:00:00Z","eventType":"item.created","data":{"externalId":"Y","rheoItemId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","platform":"rheo","salePrice":0,"currency":"SEK","type":"container"}}
            """;
        const string endpointSecret = "whsec_endpoint_specific_secret_000000000";

        // Client has no webhook secret — caller supplies the receiving endpoint's secret(s).
        using var client = new RheoClient(new RheoClientOptions { ApiKey = "key" });
        var sig = Sign(payload, endpointSecret);

        var evt = client.Webhooks.Verify(payload, sig, "whsec_wrong_rotated_out", endpointSecret);

        var created = Assert.IsType<ItemCreatedEvent>(evt);
        Assert.Equal("Y", created.Data.ExternalId);
        Assert.Equal("container", created.Data.Type);
    }
}
