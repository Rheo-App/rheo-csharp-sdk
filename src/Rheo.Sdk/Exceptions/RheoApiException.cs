namespace Rheo.Sdk.Exceptions;

public class RheoApiException : Exception
{
    public int Status { get; }
    public string? Code { get; }
    public string? RequestId { get; }

    public RheoApiException(string message, int status, string? code = null, string? requestId = null)
        : base(message)
    {
        Status = status;
        Code = code;
        RequestId = requestId;
    }
}
