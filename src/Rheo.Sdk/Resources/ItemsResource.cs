using System.Net.Http.Json;
using System.Text.Json;
using Rheo.Sdk.Types;

namespace Rheo.Sdk.Resources;

public sealed class ItemsResource
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    internal ItemsResource(HttpClient http, JsonSerializerOptions json)
    {
        _http = http;
        _json = json;
    }

    public async Task<UpsertItemResponse> UpsertAsync(
        string externalId,
        UpsertItemRequest data,
        CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}";
        using var response = await _http.PutAsJsonAsync(path, data, _json, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<UpsertItemResponse>(_json, ct))!;
    }

    public async Task<ItemStatusResponse> GetAsync(string externalId, CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}";
        using var response = await _http.GetAsync(path, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<ItemStatusResponse>(_json, ct))!;
    }

    public async Task DeleteAsync(string externalId, CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}";
        using var response = await _http.DeleteAsync(path, ct);
        response.EnsureRheoSuccess();
    }

    public async Task UpdatePriceAsync(
        string externalId,
        UpdatePriceRequest data,
        CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}/price";
        using var request = new HttpRequestMessage(HttpMethod.Patch, path)
        {
            Content = JsonContent.Create(data, options: _json),
        };
        using var response = await _http.SendAsync(request, ct);
        response.EnsureRheoSuccess();
    }

    public async Task UpdateStatusAsync(
        string externalId,
        UpdateStatusRequest data,
        CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}/status";
        using var request = new HttpRequestMessage(HttpMethod.Patch, path)
        {
            Content = JsonContent.Create(data, options: _json),
        };
        using var response = await _http.SendAsync(request, ct);
        response.EnsureRheoSuccess();
    }

    public async Task<BatchUpsertResponse> BatchUpsertAsync(
        BatchUpsertRequest data,
        CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/integration/v1/items/batch", data, _json, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<BatchUpsertResponse>(_json, ct))!;
    }

    public async Task<ItemListResponse> ListAsync(
        ListItemsParams? parameters = null,
        CancellationToken ct = default)
    {
        var query = BuildListQuery(parameters);
        using var response = await _http.GetAsync($"/integration/v1/items{query}", ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<ItemListResponse>(_json, ct))!;
    }

    public async Task<ItemSummaryResponse> SummaryAsync(string externalId, CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}/summary";
        using var response = await _http.GetAsync(path, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<ItemSummaryResponse>(_json, ct))!;
    }

    public async Task<ItemHistoryResponse> HistoryAsync(string externalId, CancellationToken ct = default)
    {
        var path = $"/integration/v1/items/{Uri.EscapeDataString(externalId)}/history";
        using var response = await _http.GetAsync(path, ct);
        response.EnsureRheoSuccess();
        return (await response.Content.ReadFromJsonAsync<ItemHistoryResponse>(_json, ct))!;
    }

    private static string BuildListQuery(ListItemsParams? p)
    {
        if (p is null) return string.Empty;

        var parts = new List<string>();
        if (p.Status is not null) parts.Add($"status={Uri.EscapeDataString(p.Status.ToString()!.ToLowerInvariant())}");
        if (p.ParentExternalId is not null) parts.Add($"parent_external_id={Uri.EscapeDataString(p.ParentExternalId)}");
        if (p.UpdatedSince is not null) parts.Add($"updated_since={Uri.EscapeDataString(p.UpdatedSince.Value.ToString("O"))}");
        if (p.Limit is not null) parts.Add($"limit={p.Limit}");
        if (p.Cursor is not null) parts.Add($"cursor={Uri.EscapeDataString(p.Cursor)}");

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}
