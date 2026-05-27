using System.Text.Json.Serialization;

namespace Rheo.Sdk.Types;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ItemCreatedEvent), "item.created")]
[JsonDerivedType(typeof(ItemImagesReadyEvent), "item.images_ready")]
[JsonDerivedType(typeof(ItemAIPricedEvent), "item.ai_priced")]
[JsonDerivedType(typeof(ItemAIListedEvent), "item.ai_listed")]
[JsonDerivedType(typeof(ListingCreatedEvent), "listing.created")]
[JsonDerivedType(typeof(ListingEndedEvent), "listing.ended")]
[JsonDerivedType(typeof(ListingFailedEvent), "listing.failed")]
[JsonDerivedType(typeof(ItemSoldEvent), "item.sold")]
[JsonDerivedType(typeof(ItemStatusChangedEvent), "item.status_changed")]
[JsonDerivedType(typeof(WorkflowApprovalPendingEvent), "workflow.approval_pending")]
[JsonDerivedType(typeof(WorkflowStepCompletedEvent), "workflow.step_completed")]
[JsonDerivedType(typeof(WorkflowRunCompletedEvent), "workflow.run_completed")]
public abstract class RheoEvent
{
    public required string Id { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string ApiVersion { get; init; }
    public abstract string Type { get; }
}

public sealed class ItemCreatedEvent : RheoEvent
{
    public override string Type => "item.created";
    public required string ExternalId { get; init; }
}

public sealed class ItemImagesReadyEvent : RheoEvent
{
    public override string Type => "item.images_ready";
    public required string ExternalId { get; init; }
    public int ImageCount { get; init; }
}

public sealed class ItemAIPricedEvent : RheoEvent
{
    public override string Type => "item.ai_priced";
    public required string ExternalId { get; init; }
    public double AiPrice { get; init; }
}

public sealed class ItemAIListedEvent : RheoEvent
{
    public override string Type => "item.ai_listed";
    public required string ExternalId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
}

public sealed class ListingCreatedEvent : RheoEvent
{
    public override string Type => "listing.created";
    public required string ExternalId { get; init; }
    public required string Platform { get; init; }
    public required string PlatformAdId { get; init; }
    public string? PlatformAdUrl { get; init; }
}

public sealed class ListingEndedEvent : RheoEvent
{
    public override string Type => "listing.ended";
    public required string ExternalId { get; init; }
    public required string Platform { get; init; }
}

public sealed class ListingFailedEvent : RheoEvent
{
    public override string Type => "listing.failed";
    public required string ExternalId { get; init; }
    public required string Platform { get; init; }
    public required string Reason { get; init; }
}

public sealed class ItemSoldEvent : RheoEvent
{
    public override string Type => "item.sold";
    public required string ExternalId { get; init; }
    public double SalePrice { get; init; }
    public required string Platform { get; init; }
    public string? BuyerCountry { get; init; }
}

public sealed class ItemStatusChangedEvent : RheoEvent
{
    public override string Type => "item.status_changed";
    public required string ExternalId { get; init; }
    public required string FromStatus { get; init; }
    public required string ToStatus { get; init; }
}

public sealed class WorkflowApprovalPendingEvent : RheoEvent
{
    public override string Type => "workflow.approval_pending";
    public required string RunId { get; init; }
    public required string WorkflowId { get; init; }
    public required string NodeId { get; init; }
    public required string Prompt { get; init; }
    public string? SubjectExternalId { get; init; }
}

public sealed class WorkflowStepCompletedEvent : RheoEvent
{
    public override string Type => "workflow.step_completed";
    public required string RunId { get; init; }
    public required string WorkflowId { get; init; }
    public required string NodeId { get; init; }
    public string? SubjectExternalId { get; init; }
}

public sealed class WorkflowRunCompletedEvent : RheoEvent
{
    public override string Type => "workflow.run_completed";
    public required string RunId { get; init; }
    public required string WorkflowId { get; init; }
    public required string Status { get; init; }
    public string? SubjectExternalId { get; init; }
}
