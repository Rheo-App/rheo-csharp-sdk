using System.Net.Http.Json;
using System.Text.Json;
using Rheo.Sdk.Types;
using Rheo.Sdk;

namespace Rheo.Sdk.Resources;

public sealed class ItemsResource
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;
    private readonly string? _partnerAccount;

    internal ItemsResource(HttpClient http, JsonSerializerOptions json, string? partnerAccount = null)
    {
        _http = http;
        _json = json;
        _partnerAccount = partnerAccount;
    }

    public async Task<UpsertItemResponse> UpsertAsync(
        string externalId,
        UpsertItemRequest data,
        CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}";
        using var response = await SendAsync(HttpMethod.Put, path, data, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<UpsertItemResponse>(_json, ct))!;
    }

    public async Task<ItemStatusResponse> GetAsync(string externalId, CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}";
        using var response = await SendAsync(HttpMethod.Get, path, null, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<ItemStatusResponse>(_json, ct))!;
    }

    public async Task DeleteAsync(string externalId, CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}";
        using var response = await SendAsync(HttpMethod.Delete, path, null, ct);
        response.EnsureRheoSuccess();
    }

    public async Task UpdatePriceAsync(
        string externalId,
        UpdatePriceRequest data,
        CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}/price";
        using var response = await SendAsync(HttpMethod.Patch, path, data, ct);
        response.EnsureRheoSuccess();
    }

    public async Task UpdateStatusAsync(
        string externalId,
        UpdateStatusRequest data,
        CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}/status";
        using var response = await SendAsync(HttpMethod.Patch, path, data, ct);
        response.EnsureRheoSuccess();
    }

    public async Task<BatchUpsertResponse> BatchUpsertAsync(
        BatchUpsertRequest data,
        CancellationToken ct = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "/integration/v1/items/batch", data, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<BatchUpsertResponse>(_json, ct))!;
    }

    public async Task<ItemListResponse> ListAsync(
        ListItemsParams? parameters = null,
        CancellationToken ct = default)
    {
        var query = BuildListQuery(parameters);
        using var response = await SendAsync(HttpMethod.Get, $"/integration/v1/items{query}", null, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<ItemListResponse>(_json, ct))!;
    }

    public async Task<ItemSummaryResponse> SummaryAsync(string externalId, CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}/summary";
        using var response = await SendAsync(HttpMethod.Get, path, null, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<ItemSummaryResponse>(_json, ct))!;
    }

    public async Task<ItemHistoryResponse> HistoryAsync(string externalId, CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}/history";
        using var response = await SendAsync(HttpMethod.Get, path, null, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<ItemHistoryResponse>(_json, ct))!;
    }

    /// <summary>Lists the child items under a container (e.g. parts under a donor vehicle).</summary>
    public async Task<ItemChildrenResponse> ChildrenAsync(string externalId, CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}/children";
        using var response = await SendAsync(HttpMethod.Get, path, null, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<ItemChildrenResponse>(_json, ct))!;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), options: _json);
        if (_partnerAccount is not null)
            request.Headers.Add("x-partner-account", _partnerAccount);

        // Default completion option buffers the full response, so disposing the
        // request (and its content) here is safe before the caller reads the body.
        return await _http.SendAsync(request, ct);
    }

    private static string BuildListQuery(ListItemsParams? p)
    {
        if (p is null) return string.Empty;

        var parts = new List<string>();
        // Serialize via the snake_case policy so RheoItemStatus.NotListed -> "not_listed"
        // (the backend matches exactly). `.ToString().ToLowerInvariant()` gave "notlisted"
        // and silently returned zero rows.
        if (p.Status is not null) parts.Add($"status={Uri.EscapeDataString(SnakeCaseNamingPolicy.Instance.ConvertName(p.Status.ToString()!))}");
        if (p.ParentExternalId is not null) parts.Add($"parent_external_id={Uri.EscapeDataString(p.ParentExternalId)}");
        if (p.UpdatedSince is not null) parts.Add($"updated_since={Uri.EscapeDataString(p.UpdatedSince.Value.ToString("O"))}");
        if (p.Limit is not null) parts.Add($"limit={p.Limit}");
        if (p.Cursor is not null) parts.Add($"cursor={Uri.EscapeDataString(p.Cursor)}");

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}
