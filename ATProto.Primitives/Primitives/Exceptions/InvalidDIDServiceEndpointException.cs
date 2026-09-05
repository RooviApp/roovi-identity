namespace ATProto.Primitives.Exceptions;

public sealed class InvalidDIDServiceEndpointException : Exception
{
    public object[]? Details { get; }

    public InvalidDIDServiceEndpointException(string message, params object[]? details) : base(message)
    {
        Details = details;
    }

    public InvalidDIDServiceEndpointException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public static void ThrowIfNotCompliant(string uri)
    {
        if (!IsRfc3986CompliantUri(uri))
        {
            throw new InvalidDIDServiceEndpointException("The provided URI is not RFC3986-compliant", uri);
        }
    }

    public static void ThrowIfNotCompliant(Dictionary<string, string> map)
    {
        foreach (string uri in map.Values)
        {
            if (!IsRfc3986CompliantUri(uri))
            {
                throw new InvalidDIDServiceEndpointException("The provided URI is not RFC3986-compliant", uri);
            }
        }
    }

    public static void ThrowIfNotCompliant(List<object> array)
    {
        foreach (var item in array)
        {
            if (item is string uri)
            {
                ThrowIfNotCompliant(uri);
            }
            else if (item is Dictionary<string, string> dictionary)
            {
                ThrowIfNotCompliant(dictionary);
            }
            else
            {
                throw new InvalidDIDServiceEndpointException("Service Endpoint map value contains invalid type", item.GetType());
            }
        }
    }

    private static bool IsRfc3986CompliantUri(string uri) => Uri.TryCreate(uri, UriKind.Absolute, out _);
}
