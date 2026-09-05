namespace ATProto.Primitives.Exceptions;

public sealed class InvalidDIDException : Exception
{
    public DID DID { get; init; }

    public InvalidDIDException(DID did, string message)
        : base(message)
    {
        DID = did;
    }

    public InvalidDIDException(DID did, string message, Exception innerException)
        : base(message, innerException)
    {
        DID = did;
    }
}
