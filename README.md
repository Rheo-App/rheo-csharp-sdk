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
    Domain = new AutoPartsDomain
    {
        Vehicle = new DonorVehicle { Manufacturer = "Volvo", Model = "XC90", Year = 2018, VehicleType = "Bil" },
        Part = new PartInfo { Name = "Bromsok fram vänster", OemNumber = "31400452" },
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
        Console.WriteLine($"{sold.Data.ExternalId} sold for {sold.Data.SalePrice} SEK");

    return Results.Ok();
});
```

---

## Reseller routing (many accounts, one key)

If you manage several Rheo accounts under one reseller API key (e.g. a chain of
dismantling yards), use `ForAccount` to route calls to a specific member. The value is
the reseller's `external_id` for that member — the reference set in the business
**Managed accounts** UI. It sends `x-partner-account` per request, overriding any
client-level `PartnerAccount`:

```csharp
await rheo.ForAccount("10").Items.UpsertAsync("10-PART-001", data);
await rheo.ForAccount("10").Items.UpdatePriceAsync("10-PART-001", new UpdatePriceRequest { Price = 800 });
var yard = await rheo.ForAccount("10").Items.ListAsync(new ListItemsParams { Status = RheoItemStatus.Active });
```

Set `PartnerAccount` on `RheoClientOptions` to scope *every* call to one account instead.

---

## Webhooks

Each webhook endpoint has its **own signing secret** — verify with the secret of the
endpoint that received the event. Pass it explicitly, or set one `WebhookSecret` on the
client. Pass several to accept any during rotation:

```csharp
var evt = rheo.Webhooks.Verify(rawBody, signature, endpointSecret);
var rotating = rheo.Webhooks.Verify(rawBody, signature, oldSecret, newSecret);
```

Events carry an additive `Data.Account`. On a reseller endpoint (`scope: members`),
`Account.MemberExternalId` tells you which member the event belongs to:

```csharp
if (evt is ItemSoldEvent sold)
    DecrementStock(sold.Data.Account?.MemberExternalId, sold.Data.ExternalId);
```

Delivered event types: `item.created`, `item.images_ready`, `listing.created`,
`listing.ended`, `listing.failed`, `item.sold`.

---

## Features

- **Typed items resource** — UpsertAsync, GetAsync, DeleteAsync, UpdatePriceAsync, UpdateStatusAsync, BatchUpsertAsync (up to 500 items), ListAsync with cursor pagination, SummaryAsync, HistoryAsync, ChildrenAsync
- **All methods accept `CancellationToken`**
- **Webhook verification** — HMAC-SHA256, timing-safe (`CryptographicOperations.FixedTimeEquals`)
- **Automatic retry** — exponential backoff on 429 / 5xx, honours `Retry-After`
- **Typed exceptions** — `RheoApiException`, `RheoRateLimitException`, `RheoWebhookSignatureException`
- **Vehicle hierarchy** — model donor vehicles as containers; parts reference their vehicle via `ParentExternalId`, list parts with `ChildrenAsync`
- **Reseller routing** — `rheo.ForAccount("10").Items…` per-account, or set `PartnerAccount` client-wide
- **DI-friendly** — `RheoClient` implements `IDisposable`; register as singleton

---

## Publishing

```bash
# Tag and push — GitHub Actions publishes automatically
git tag v0.7.0
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
