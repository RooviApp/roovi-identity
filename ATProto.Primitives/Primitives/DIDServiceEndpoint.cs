using ATProto.Primitives.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace ATProto.Primitives;

/// <summary>
/// A DID service endpoint MUST be a string, a map, or a set composed of one or more strings and/or maps.
/// All string values MUST be valid URIs conforming to [RFC3986] and normalized according to the Normalization and Comparison rules in RFC3986 and to any normalization rules in its application URI scheme specification.
/// </summary>
public sealed class DIDServiceEndpoint
{
    public enum EndpointType
    {
        Uri,
        Map,
        Set,
    }

    public EndpointType Type { get; set; }
    public string? Uri { get; set; }
    public Dictionary<string, string>? Map { get; set; }
    public List<object>? Set { get; set; }

    private DIDServiceEndpoint(string singleUrl)
    {
        InvalidDIDServiceEndpointException.ThrowIfNotCompliant(singleUrl);

        Type = EndpointType.Uri;
        Uri = singleUrl;
    }

    private DIDServiceEndpoint(Dictionary<string, string> map)
    {
        InvalidDIDServiceEndpointException.ThrowIfNotCompliant(map);

        Type = EndpointType.Map;
        Map = new Dictionary<string, string>(map);
    }

    private DIDServiceEndpoint(List<object> set)
    {
        InvalidDIDServiceEndpointException.ThrowIfNotCompliant(set);

        Type = EndpointType.Set;
        Set = new List<object>(set);
    }

    public static DIDServiceEndpoint FromUri([NotNull] string uri)
    {
        return new DIDServiceEndpoint(uri);
    }

    public static DIDServiceEndpoint FromMap(Dictionary<string, string> map)
    {
        return new DIDServiceEndpoint(map);
    }

    public static DIDServiceEndpoint FromSet(List<object> set)
    {
        return new DIDServiceEndpoint(set);
    }
}
