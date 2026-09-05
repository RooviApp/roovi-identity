using ATProto.Primitives.Exceptions;

namespace ATProto.Tests.Primitives.Exceptions;

public class InvalidDIDServiceEndpointExceptionTests
{
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com")]
    [InlineData("ftp://example.com")]
    public void ThrowIfNotCompliant_ValidUri_DoesNotThrow(string validUri)
    {
        var exception = Record.Exception(() => InvalidDIDServiceEndpointException.ThrowIfNotCompliant(validUri));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("invalid_uri")]
    [InlineData("http:/example.com")]
    [InlineData("")]
    public void ThrowIfNotCompliant_InvalidUri_ThrowsException(string invalidUri)
    {
        var exception = Assert.Throws<InvalidDIDServiceEndpointException>(
            () => InvalidDIDServiceEndpointException.ThrowIfNotCompliant(invalidUri)
        );
        Assert.Equal($"The provided URI is not RFC3986-compliant", exception.Message);
        Assert.NotNull(exception.Details);
        Assert.Contains(invalidUri, exception.Details);
    }

    [Fact]
    public void ThrowIfNotCompliant_ValidUriMap_DoesNotThrow()
    {
        var validMap = new Dictionary<string, string>
        {
            { "key1", "https://example.com" },
            { "key2", "http://example.org" }
        };

        var exception = Record.Exception(() => InvalidDIDServiceEndpointException.ThrowIfNotCompliant(validMap));
        Assert.Null(exception);
    }

    [Fact]
    public void ThrowIfNotCompliant_InvalidUriMap_ThrowsException()
    {
        var map = new Dictionary<string, string>
        {
            { "key1", "https://example.com" },
            { "key2", "invalid_uri" }
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDServiceEndpointException>(
            () => InvalidDIDServiceEndpointException.ThrowIfNotCompliant(map)
        );
        Assert.Equal($"The provided URI is not RFC3986-compliant", exception.Message);
        Assert.NotNull(exception.Details);
        Assert.Contains("invalid_uri", exception.Details);
    }

    [Fact]
    public void ThrowIfNotCompliant_ValidArray_DoesNotThrow()
    {
        var validArray = new List<object>
        {
            "https://example.com",
            new Dictionary<string, string> { { "key", "http://example.org" } }
        };

        var exception = Record.Exception(() => InvalidDIDServiceEndpointException.ThrowIfNotCompliant(validArray));
        Assert.Null(exception);
    }

    [Fact]
    public void ThrowIfNotCompliant_InvalidArray_ThrowsExceptionForInvalidUri()
    {
        var array = new List<object>
        {
            "https://example.com",
            new Dictionary<string, string> { { "key", "invalid_uri" } }
        };

        var exception = Assert.Throws<InvalidDIDServiceEndpointException>(
            () => InvalidDIDServiceEndpointException.ThrowIfNotCompliant(array)
        );
        Assert.Equal($"The provided URI is not RFC3986-compliant", exception.Message);
        Assert.NotNull(exception.Details);
        Assert.Contains("invalid_uri", exception.Details);
    }

    [Fact]
    public void ThrowIfNotCompliant_InvalidArray_ThrowsExceptionForInvalidType()
    {
        var array = new List<object>
        {
            "https://example.com",
            12345 // Invalid type
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDServiceEndpointException>(
            () => InvalidDIDServiceEndpointException.ThrowIfNotCompliant(array)
        );
        Assert.Equal("Service Endpoint map value contains invalid type", exception.Message);
        Assert.NotNull(exception.Details);
        Assert.Contains(typeof(int), exception.Details);
    }
}
