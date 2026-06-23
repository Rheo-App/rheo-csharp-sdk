using Rheo.Sdk.Exceptions;

namespace Rheo.Sdk;

internal sealed class RetryHandler : DelegatingHandler
{
    private readonly int _maxRetries;

    public RetryHandler(int maxRetries) => _maxRetries = maxRetries;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct)
    {
        HttpResponseMessage? response = null;

        // 429 is always safe to retry (rejected before processing). 5xx is retried
        // only for idempotent methods — retrying a POST (e.g. orders/{id}/tracking)
        // after the server committed it would double-create.
        var method = request.Method;
        var idempotent = method == HttpMethod.Get || method == HttpMethod.Head
            || method == HttpMethod.Put || method == HttpMethod.Delete;

        for (var attempt = 0; attempt <= _maxRetries; attempt++)
        {
            response = await base.SendAsync(request, ct);

            var status = (int)response.StatusCode;
            var isRetryable = status == 429 || (idempotent && status >= 500);

            if (!isRetryable || attempt >= _maxRetries)
                return response;

            var delay = response.Headers.RetryAfter?.Delta
                ?? TimeSpan.FromMilliseconds(Math.Min(200 * Math.Pow(2, attempt), 10_000));

            response.Dispose();
            await Task.Delay(delay, ct);
        }

        return response!;
    }
}
