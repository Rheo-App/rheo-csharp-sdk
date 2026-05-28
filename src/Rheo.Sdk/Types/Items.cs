namespace Rheo.Sdk.Types;

public class UpsertItemRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    /// <summary>Required for <c>Type = Item</c>; ignored (defaults to 0) for <c>Type = Container</c>.</summary>
    public double? Price { get; init; }
    public double? ShippingCost { get; init; }
    public IReadOnlyList<string>? ImageUrls { get; init; }
    public DomainObject? Domain { get; init; }
    /// <summary>Semver schema version for the domain payload. Defaults to "1.0.0" if omitted.</summary>
    public string? SchemaVersion { get; init; }
    /// <summary>Links this item to a parent container (e.g. the donor vehicle's external id). The parent must already exist.</summary>
    public string? ParentExternalId { get; init; }
    /// <summary><c>Item</c> (default, sellable) or <c>Container</c> (non-sellable grouping node, e.g. a donor vehicle).</summary>
    public RheoItemType? Type { get; init; }
    /// <summary>Arbitrary integrator key/value pairs. Stored and returned verbatim. Max 50 keys, 1 KB per value.</summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
    public bool? AutoPublishTradera { get; init; }
    public bool? UseAiEnhancement { get; init; }
    public double? WeightKg { get; init; }
    public string? Currency { get; init; }
}

public sealed class UpsertItemResponse
{
    public bool Success { get; init; }
    public required string ItemId { get; init; }
    public required string Message { get; init; }
}

public sealed class ItemStatusResponse
{
    public required string ExternalId { get; init; }
    public required string RheoItemId { get; init; }
    public RheoItemStatus? RheoStatus { get; init; }
    public string? TraderaStatus { get; init; }
    public string? TraderaAdId { get; init; }
    public string? TraderaAdUrl { get; init; }
    public double? Price { get; init; }
}

public sealed class UpdatePriceRequest
{
    public required double Price { get; init; }
}

public sealed class UpdateStatusRequest
{
    public required string Status { get; init; }
}

public sealed class BatchUpsertItem : UpsertItemRequest
{
    public required string ExternalId { get; init; }
}

public sealed class BatchUpsertRequest
{
    public required IReadOnlyList<BatchUpsertItem> Items { get; init; }
}

public sealed class BatchUpsertError
{
    public required string ExternalId { get; init; }
    public required string Reason { get; init; }
}

public sealed class BatchUpsertResponse
{
    public long Accepted { get; init; }
    public long Rejected { get; init; }
    public required IReadOnlyList<BatchUpsertError> Errors { get; init; }
}

public sealed class ListItemsParams
{
    public RheoItemStatus? Status { get; init; }
    public string? ParentExternalId { get; init; }
    public DateTimeOffset? UpdatedSince { get; init; }
    /// <summary>Page size, default 100, max 1000.</summary>
    public int? Limit { get; init; }
    /// <summary>Opaque cursor from a previous response's <c>NextCursor</c>.</summary>
    public string? Cursor { get; init; }
}

public sealed class ListItem
{
    public string? ExternalId { get; init; }
    public required string RheoItemId { get; init; }
    public RheoItemStatus? Status { get; init; }
    public double? Price { get; init; }
    public string? Title { get; init; }
    public RheoItemType Type { get; init; }
    public string? ParentExternalId { get; init; }
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
    public string? TraderaAdId { get; init; }
    public string? TraderaAdUrl { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class ItemListResponse
{
    public required IReadOnlyList<ListItem> Items { get; init; }
    /// <summary>Pass back as <c>Cursor</c> to fetch the next page. Null when there are no more.</summary>
    public string? NextCursor { get; init; }
    /// <summary>Total rows matching the filters (ignores cursor/limit).</summary>
    public long Total { get; init; }
}

/// <summary>Aggregate over a container's direct children (the summary endpoint).</summary>
public sealed class ItemSummaryResponse
{
    public required string ExternalId { get; init; }
    public long ChildCount { get; init; }
    public long ActiveCount { get; init; }
    public long SoldCount { get; init; }
    public long DraftCount { get; init; }
    /// <summary>Sum of marketplace price across active (listed) children.</summary>
    public double ListedValue { get; init; }
    /// <summary>Sum of marketplace price across sold children (approximate gross).</summary>
    public double SoldValue { get; init; }
    public required string Currency { get; init; }
}

/// <summary>One lifecycle milestone in an item's history.</summary>
public sealed class ItemHistoryEvent
{
    /// <summary>e.g. item.created, item.images_ready, price.updated, listing.created, item.sold, item.unlisted.</summary>
    public required string EventType { get; init; }
    /// <summary>Event-specific detail, verbatim (e.g. { price }, { imagesProcessed }, { traderaAdId }).</summary>
    public IReadOnlyDictionary<string, object?>? Data { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class ItemHistoryResponse
{
    public required string ExternalId { get; init; }
    public required string RheoItemId { get; init; }
    public long Count { get; init; }
    /// <summary>Newest first.</summary>
    public required IReadOnlyList<ItemHistoryEvent> Events { get; init; }
}

/// <summary>One child item under a container (e.g. a part under a donor vehicle).</summary>
public sealed class ChildItem
{
    public string? ExternalId { get; init; }
    public required string RheoItemId { get; init; }
    public string? Title { get; init; }
    public RheoItemStatus? Status { get; init; }
    public double? Price { get; init; }
    public RheoItemType Type { get; init; }
}

public sealed class ItemChildrenResponse
{
    public required string ParentExternalId { get; init; }
    public long Count { get; init; }
    public required IReadOnlyList<ChildItem> Children { get; init; }
}
