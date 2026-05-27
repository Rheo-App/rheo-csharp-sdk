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

        for (var attempt = 0; attempt <= _maxRetries; attempt++)
        {
            response = await base.SendAsync(request, ct);

            var status = (int)response.StatusCode;
            var isRetryable = status == 429 || status >= 500;

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
