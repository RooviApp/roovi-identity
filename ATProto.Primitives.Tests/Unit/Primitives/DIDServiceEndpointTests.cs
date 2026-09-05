using ATProto.Primitives;
using ATProto.Primitives.Exceptions;

namespace ATProto.Tests.Primitives;

public class DIDServiceEndpointTests
{
    [Fact]
    public void FromUri_ValidUri_CreatesEndpointWithTypeUri()
    {
        // Arrange
        var validUri = "https://example.com";

        // Act
        var endpoint = DIDServiceEndpoint.FromUri(validUri);

        // Assert
        Assert.Equal(DIDServiceEndpoint.EndpointType.Uri, endpoint.Type);
        Assert.Equal(validUri, endpoint.Uri);
        Assert.Null(endpoint.Map);
        Assert.Null(endpoint.Set);
    }

    [Theory]
    [InlineData("invalid_uri")]
    [InlineData("")]
    public void FromUri_InvalidUri_ThrowsException(string invalidUri)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidDIDServiceEndpointException>(
            () => DIDServiceEndpoint.FromUri(invalidUri)
        );
        Assert.Equal($"The provided URI is not RFC3986-compliant", exception.Message);
        Assert.NotNull(exception.Details);
        Assert.Contains(invalidUri, exception.Details);
    }

    [Fact]
    public void FromMap_ValidMap_CreatesEndpointWithTypeMap()
    {
        // Arrange
        var validMap = new Dictionary<string, string>
        {
            { "key1", "https://example.com" },
            { "key2", "http://example.org" }
        };

        // Act
        var endpoint = DIDServiceEndpoint.FromMap(validMap);

        // Assert
        Assert.Equal(DIDServiceEndpoint.EndpointType.Map, endpoint.Type);
        Assert.Equal(validMap, endpoint.Map);
        Assert.Null(endpoint.Uri);
        Assert.Null(endpoint.Set);
    }

    [Fact]
    public void FromMap_InvalidMap_ThrowsException()
    {
        // Arrange
        var invalidMap = new Dictionary<string, string>
        {
            { "key1", "https://example.com" },
            { "key2", "invalid_uri" }
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDServiceEndpointException>(
            () => DIDServiceEndpoint.FromMap(invalidMap)
        );
        Assert.Equal($"The provided URI is not RFC3986-compliant", exception.Message);
        Assert.NotNull(exception.Details);
        Assert.Contains("invalid_uri", exception.Details);
    }

    [Fact]
    public void FromSet_ValidSet_CreatesEndpointWithTypeSet()
    {
        // Arrange
        var validSet = new List<object>
        {
            "https://example.com",
            new Dictionary<string, string> { { "key", "http://example.org" } }
        };

        // Act
        var endpoint = DIDServiceEndpoint.FromSet(validSet);

        // Assert
        Assert.Equal(DIDServiceEndpoint.EndpointType.Set, endpoint.Type);
        Assert.Equal(validSet, endpoint.Set);
        Assert.Null(endpoint.Uri);
        Assert.Null(endpoint.Map);
    }

    [Fact]
    public void FromSet_InvalidSet_ThrowsExceptionForInvalidUri()
    {
        // Arrange
        var invalidSet = new List<object>
        {
            "https://example.com",
            new Dictionary<string, string> { { "key", "invalid_uri" } }
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDServiceEndpointException>(
            () => DIDServiceEndpoint.FromSet(invalidSet)
        );
        Assert.Equal($"The provided URI is not RFC3986-compliant", exception.Message);
        Assert.NotNull(exception.Details);
        Assert.Contains("invalid_uri", exception.Details);
    }

    [Fact]
    public void FromSet_InvalidSet_ThrowsExceptionForInvalidType()
    {
        // Arrange
        var invalidSet = new List<object>
        {
            "https://example.com",
            12345 // Invalid type
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDServiceEndpointException>(
            () => DIDServiceEndpoint.FromSet(invalidSet)
        );
        Assert.Equal("Service Endpoint map value contains invalid type", exception.Message);
        Assert.NotNull(exception.Details);
        Assert.Contains(typeof(int), exception.Details);
    }
}
