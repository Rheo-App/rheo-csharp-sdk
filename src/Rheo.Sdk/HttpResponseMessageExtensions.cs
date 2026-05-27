using System.Net.Http.Json;
using System.Text.Json;
using Rheo.Sdk.Exceptions;

namespace Rheo.Sdk;

internal static class HttpResponseMessageExtensions
{
    internal static void EnsureRheoSuccess(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var status = (int)response.StatusCode;
        string? requestId = null;
        if (response.Headers.TryGetValues("x-request-id", out var ids))
            requestId = ids.FirstOrDefault();

        if (status == 429)
        {
            TimeSpan? retryAfter = null;
            if (response.Headers.RetryAfter?.Delta is { } delta)
                retryAfter = delta;
            throw new RheoRateLimitException(retryAfter);
        }

        // Attempt to read a structured error body; fall back to status text.
        string message;
        try
        {
            using var doc = JsonDocument.Parse(
                response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            message = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() ?? $"HTTP {status}"
                    : doc.RootElement.TryGetProperty("error", out var e) ? e.GetString() ?? $"HTTP {status}"
                    : $"HTTP {status}";
        }
        catch
        {
            message = $"HTTP {status}";
        }

        throw new RheoApiException(message, status, requestId: requestId);
    }
}
