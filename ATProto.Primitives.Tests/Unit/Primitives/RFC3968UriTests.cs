using ATProto.Primitives;

namespace ATProto.Tests.Primitives;

public class RFC3968UriTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path?query=1#fragment")]
    [InlineData("ftp://ftp.example.com/resource.txt")]
    public void Parse_ValidUri_ReturnsRFC3968Uri(string validUri)
    {
        var rfc3968Uri = RFC3968Uri.Parse(validUri);
        Assert.NotNull(rfc3968Uri);
        Assert.Equal(validUri, rfc3968Uri.Value);
    }

    [Theory]
    [InlineData("invalidUri")]
    [InlineData("http://")]
    [InlineData("://example.com")]
    public void Parse_InvalidUri_ThrowsArgumentException(string invalidUri)
    {
        var exception = Assert.Throws<ArgumentException>(() => RFC3968Uri.Parse(invalidUri));
        Assert.Equal($"Invalid RFC3968Uri format: {invalidUri} (Parameter 'value')", exception.Message);
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path?query=1#fragment")]
    public void Equals_SameValue_ReturnsTrue(string uriValue)
    {
        var uri1 = RFC3968Uri.Parse(uriValue);
        var uri2 = RFC3968Uri.Parse(uriValue);

        bool result = uri1.Equals(uri2);
        Assert.True(result);
    }

    [Theory]
    [InlineData("http://example.com", "http://example.org")]
    public void Equals_DifferentValue_ReturnsFalse(string uriValue1, string uriValue2)
    {
        var uri1 = RFC3968Uri.Parse(uriValue1);
        var uri2 = RFC3968Uri.Parse(uriValue2);

        bool result = uri1.Equals(uri2);
        Assert.False(result);
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var uri = RFC3968Uri.Parse("http://example.com");

        bool result = uri.Equals(null);
        Assert.False(result);
    }

    [Theory]
    [InlineData("http://example.com")]
    public void GetHashCode_SameValue_ReturnsSameHash(string uriValue)
    {
        var uri1 = RFC3968Uri.Parse(uriValue);
        var uri2 = RFC3968Uri.Parse(uriValue);

        int hash1 = uri1.GetHashCode();
        int hash2 = uri2.GetHashCode();
        Assert.Equal(hash1, hash2);
    }

    [Theory]
    [InlineData("http://example.com")]
    public void ToString_ReturnsValue(string uriValue)
    {
        var rfc3968Uri = RFC3968Uri.Parse(uriValue);

        string result = rfc3968Uri.ToString();
        Assert.Equal(uriValue, result);
    }

    [Theory]
    [InlineData("http://example.com")]
    public void EqualityOperator_SameValue_ReturnsTrue(string uriValue)
    {
        var uri1 = RFC3968Uri.Parse(uriValue);
        var uri2 = RFC3968Uri.Parse(uriValue);

        bool result = uri1 == uri2;
        Assert.True(result);
    }

    [Theory]
    [InlineData("http://example.com", "http://example.org")]
    public void InequalityOperator_DifferentValue_ReturnsTrue(string uriValue1, string uriValue2)
    {
        var uri1 = RFC3968Uri.Parse(uriValue1);
        var uri2 = RFC3968Uri.Parse(uriValue2);

        bool result = uri1 != uri2;
        Assert.True(result);
    }
}
