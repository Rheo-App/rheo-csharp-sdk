using System.Text.Json.Serialization;

namespace Rheo.Sdk.Types;

// Webhook event types — typed mirror of the payloads Rheo POSTs to your webhook
// endpoints (rheo-market/src/worker/handlers/integration.rs). The envelope is
// { eventId, eventType, timestamp, data }; `data` carries the shared fields plus
// an additive `account` object and per-event metadata.

/// <summary>
/// Identifies which Rheo account an event belongs to. <see cref="RheoUserId"/> is
/// always the owning account. <see cref="MemberExternalId"/> is present only when
/// the event is delivered to a reseller endpoint (<c>scope='members'</c>) — the
/// reseller's own reference for the member, so one endpoint can route per yard.
/// </summary>
public sealed class EventAccount
{
    public required string RheoUserId { get; init; }
    public string? MemberExternalId { get; init; }
}

/// <summary>The `data` object of a webhook event. Shared fields are always present;
/// the metadata fields are populated depending on <c>eventType</c>.</summary>
public sealed class EventData
{
    /// <summary>The integrator's external id for the item (unchanged across all events).</summary>
    public required string ExternalId { get; init; }
    /// <summary>Rheo's internal item UUID.</summary>
    public Guid RheoItemId { get; init; }
    /// <summary>Origin platform, e.g. <c>tradera</c>, <c>rheo</c>, <c>rheo_stripe</c>, <c>rheo_swish</c>.</summary>
    public required string Platform { get; init; }
    /// <summary>Sale price in SEK. <c>0</c> for non-sale events.</summary>
    public double SalePrice { get; init; }
    public string? Currency { get; init; }
    public EventAccount? Account { get; init; }

    /// <summary>item.created only: <c>item</c> or <c>container</c>.</summary>
    public string? Type { get; init; }
    /// <summary>item.images_ready only: number of images processed.</summary>
    public int? ImagesProcessed { get; init; }
    /// <summary>item.sold / listing.created / listing.ended: the Tradera ad id.</summary>
    public string? TraderaAdId { get; init; }
    /// <summary>listing.created only: direct link to the live Tradera ad.</summary>
    public string? TraderaAdUrl { get; init; }
    /// <summary>listing.failed only: human-readable failure cause.</summary>
    public string? Error { get; init; }
}

// Custom converter (not [JsonPolymorphic]) because the live payload puts `eventId`
// before the `eventType` discriminator, and STJ's attribute-based polymorphism
// requires the discriminator to be the first property. The converter buffers the
// object and reads `eventType` regardless of position. EventType stays a normal
// (read-only) property, so it serializes back out and is ignored on read.
[JsonConverter(typeof(RheoEventConverter))]
public abstract class RheoEvent
{
    public required string EventId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required EventData Data { get; init; }
    public abstract string EventType { get; }
}

public sealed class ItemCreatedEvent : RheoEvent
{
    public override string EventType => "item.created";
}

public sealed class ItemImagesReadyEvent : RheoEvent
{
    public override string EventType => "item.images_ready";
}

public sealed class ListingCreatedEvent : RheoEvent
{
    public override string EventType => "listing.created";
}

public sealed class ListingEndedEvent : RheoEvent
{
    public override string EventType => "listing.ended";
}

public sealed class ListingFailedEvent : RheoEvent
{
    public override string EventType => "listing.failed";
}

public sealed class ItemSoldEvent : RheoEvent
{
    public override string EventType => "item.sold";
}
