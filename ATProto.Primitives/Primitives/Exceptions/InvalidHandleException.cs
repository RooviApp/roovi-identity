namespace ATProto.Primitives.Exceptions;

public sealed class InvalidHandleException : Exception
{
    public string Handle { get; init; }

    public InvalidHandleException(string handle, string message)
        : base(message)
    {
        Handle = handle;
    }

    public InvalidHandleException(string handle, string message, Exception innerException)
        : base(message, innerException)
    {
        Handle = handle;
    }
}
