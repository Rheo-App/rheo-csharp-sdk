using System.Text.Json;
using Rheo.Sdk.Resources;

namespace Rheo.Sdk;

/// <summary>
/// A lightweight, account-scoped view of the Rheo API. Every call made through
/// <see cref="Items"/> sends the <c>x-partner-account</c> header so a reseller can
/// route requests to a specific managed member from a single client and API key.
/// Obtain one via <see cref="RheoClient.ForAccount(string)"/>. The scope reuses the
/// owning client's <see cref="HttpClient"/>; do not dispose it separately.
/// </summary>
public sealed class RheoAccountScope
{
    /// <summary>Item operations scoped to the member account.</summary>
    public ItemsResource Items { get; }

    /// <summary>Order operations (e.g. seller-shipped tracking) scoped to the member account.</summary>
    public OrdersResource Orders { get; }

    internal RheoAccountScope(HttpClient http, JsonSerializerOptions json, string memberExternalId)
    {
        Items = new ItemsResource(http, json, memberExternalId);
        Orders = new OrdersResource(http, json, memberExternalId);
    }
}
