namespace Rheo.Sdk.Types;

/// <summary>
/// Report a tracking number for an order you ship yourself (items synced with
/// <see cref="ShippingStrategy.SellerShipped"/>). Rheo relays it to the buyer and
/// releases the payout against it. The order id arrives on the <c>item.sold</c> webhook.
/// </summary>
public sealed class SellerTrackingRequest
{
    /// <summary>Carrier name, e.g. <c>postnord</c>, <c>dhl</c>, <c>schenker</c>.</summary>
    public required string Carrier { get; init; }
    /// <summary>The carrier's tracking number for the shipment.</summary>
    public required string TrackingNumber { get; init; }
}

public sealed class SellerTrackingResponse
{
    /// <summary>Rheo's shipment id (UUID) for the tracked parcel.</summary>
    public required string ShipmentId { get; init; }
    public required string TrackingNumber { get; init; }
    public required string Carrier { get; init; }
}
