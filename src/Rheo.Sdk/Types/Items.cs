namespace Rheo.Sdk.Types;

public sealed class UpsertItemRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public double? Price { get; init; }
    public double? ShippingCost { get; init; }
    public IReadOnlyList<string>? ImageUrls { get; init; }
    public DomainObject? Domain { get; init; }
    public string? ParentExternalId { get; init; }
    public RheoItemType? Type { get; init; }
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
    public int Accepted { get; init; }
    public int Rejected { get; init; }
    public required IReadOnlyList<BatchUpsertError> Errors { get; init; }
}

public sealed class ListItemsParams
{
    public RheoItemStatus? Status { get; init; }
    public string? ParentExternalId { get; init; }
    public DateTimeOffset? UpdatedSince { get; init; }
    public int? Limit { get; init; }
    public string? Cursor { get; init; }
}

public sealed class ItemListResponse
{
    public required IReadOnlyList<ItemStatusResponse> Items { get; init; }
    public string? NextCursor { get; init; }
    public int Total { get; init; }
}

public sealed class ItemSummaryChildren
{
    public int Total { get; init; }
    public required IReadOnlyDictionary<string, int> ByStatus { get; init; }
}

public sealed class ItemSummaryRevenue
{
    public double TotalSek { get; init; }
    public double PendingListedSek { get; init; }
}

public sealed class ItemSummaryResponse
{
    public required string ExternalId { get; init; }
    public required string Title { get; init; }
    public required ItemSummaryChildren Children { get; init; }
    public required ItemSummaryRevenue Revenue { get; init; }
}

public sealed class HistoryEntry
{
    public required string At { get; init; }
    public required string Event { get; init; }
    public string? Actor { get; init; }
    public IReadOnlyDictionary<string, object?>? Extra { get; init; }
}

public sealed class ItemHistoryResponse
{
    public required string ExternalId { get; init; }
    public required IReadOnlyList<HistoryEntry> History { get; init; }
}
