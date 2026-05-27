namespace Rheo.Sdk.Exceptions;

public sealed class RheoWebhookSignatureException : Exception
{
    public RheoWebhookSignatureException(string? message = null)
        : base(message ?? "Webhook signature verification failed") { }
}
