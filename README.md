# Rheo.Sdk

Official .NET SDK for the [Rheo](https://rheo.se) integration API.

Push inventory from your DMS or ERP to Tradera auctions. Receive typed webhook events when items sell.

```
dotnet add package Rheo.Sdk
```

Requires .NET 7+.

---

## Quick start

```csharp
using Rheo.Sdk;
using Rheo.Sdk.Types;

var rheo = new RheoClient(new RheoClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("RHEO_API_KEY")!,
    WebhookSecret = Environment.GetEnvironmentVariable("RHEO_WEBHOOK_SECRET"),
});

// Push an item — Rheo downloads images and publishes to Tradera
await rheo.Items.UpsertAsync("ERP_PART_12345", new UpsertItemRequest
{
    Title = "Volvo XC90 bromsok fram vänster — 2018",
    Price = 950,
    ShippingCost = 149,
    ImageUrls = ["https://cdn.example.com/parts/12345/1.jpg"],
    Domain = new AutomotivePartDomain
    {
        OemCode = "31400452",
        Manufacturer = "Volvo",
        ConditionGrade = "B",
    },
    AutoPublishTradera = true,
});

// Check status
var item = await rheo.Items.GetAsync("ERP_PART_12345");
Console.WriteLine(item.TraderaAdUrl);  // https://www.tradera.com/item/398271634

// Verify a webhook (ASP.NET Core)
app.MapPost("/webhooks/rheo", async (HttpRequest request, RheoClient rheo) =>
{
    var rawBody = await new StreamReader(request.Body).ReadToEndAsync();
    var evt = rheo.Webhooks.Verify(rawBody, request.Headers["X-Rheo-Signature"].ToString());

    if (evt is ItemSoldEvent sold)
        Console.WriteLine($"{sold.ExternalId} sold for {sold.SalePrice} SEK");

    return Results.Ok();
});
```

---

## Features

- **Typed items resource** — UpsertAsync, GetAsync, DeleteAsync, UpdatePriceAsync, UpdateStatusAsync, BatchUpsertAsync (up to 500 items), ListAsync with cursor pagination, SummaryAsync, HistoryAsync
- **All methods accept `CancellationToken`**
- **Webhook verification** — HMAC-SHA256, timing-safe (`CryptographicOperations.FixedTimeEquals`)
- **Automatic retry** — exponential backoff on 429 / 5xx, honours `Retry-After`
- **Typed exceptions** — `RheoApiException`, `RheoRateLimitException`, `RheoWebhookSignatureException`
- **Vehicle hierarchy** — model donor vehicles as containers; parts reference their vehicle via `ParentExternalId`
- **Reseller routing** — pass `PartnerAccount` to scope all calls to a member account
- **DI-friendly** — `RheoClient` implements `IDisposable`; register as singleton

---

## Publishing

```bash
# Tag and push — GitHub Actions publishes automatically
git tag v0.2.0
git push --follow-tags
```

Requires `NUGET_API_KEY` secret in GitHub repo Settings → Secrets.

To publish manually:

```bash
source .env   # loads NUGET_API_KEY
dotnet pack src/Rheo.Sdk/Rheo.Sdk.csproj -c Release -o ./nupkg
dotnet nuget push ./nupkg/*.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

---

## Documentation

Full reference at [docs.rheo.se/sdk/csharp/](https://docs.rheo.se/sdk/csharp/)

---

## License

MIT
