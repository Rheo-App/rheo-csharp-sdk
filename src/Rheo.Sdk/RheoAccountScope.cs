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

    internal RheoAccountScope(HttpClient http, JsonSerializerOptions json, string memberExternalId)
    {
        Items = new ItemsResource(http, json, memberExternalId);
    }
}
