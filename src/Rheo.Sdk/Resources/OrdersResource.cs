using System.Net.Http.Json;
using System.Text.Json;
using Rheo.Sdk.Types;

namespace Rheo.Sdk.Resources;

public sealed class OrdersResource
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;
    private readonly string? _partnerAccount;

    internal OrdersResource(HttpClient http, JsonSerializerOptions json, string? partnerAccount = null)
    {
        _http = http;
        _json = json;
        _partnerAccount = partnerAccount;
    }

    /// <summary>
    /// Report a tracking number for an order you shipped yourself (items synced with
    /// <see cref="ShippingStrategy.SellerShipped"/>). Rheo relays it to the buyer and releases
    /// the payout against it. <paramref name="orderId"/> arrives on the <c>item.sold</c> webhook.
    /// </summary>
    public async Task<SellerTrackingResponse> SubmitTrackingAsync(
        string orderId,
        SellerTrackingRequest data,
        CancellationToken ct = default)
    {
        var path = $"/integration/v1/orders/{Uri.EscapeDataString(orderId)}/tracking";
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(data, data.GetType(), options: _json),
        };
        if (_partnerAccount is not null)
            request.Headers.Add("x-partner-account", _partnerAccount);

        using var response = await _http.SendAsync(request, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<SellerTrackingResponse>(_json, ct))!;
    }
}
