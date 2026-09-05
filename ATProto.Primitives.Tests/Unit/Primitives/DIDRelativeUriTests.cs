using ATProto.Primitives;

namespace ATProto.Tests.Primitives;

public class DIDRelativeUriTests
{
    [Theory]
    [InlineData("#example")]
    [InlineData("#anotherExample")]
    public void Parse_ValidUri_ReturnsDIDRelativeUri(string validUri)
    {
        var didRelativeUri = DIDRelativeUri.Parse(validUri);
        Assert.Equal(validUri, didRelativeUri.Value);
    }

    [Theory]
    [InlineData("invalidUri")]
    [InlineData("  ")]
    public void Parse_InvalidUri_ThrowsArgumentException(string invalidUri)
    {
        var exception = Assert.Throws<ArgumentException>(() => DIDRelativeUri.Parse(invalidUri));
        Assert.Equal($"Invalid DIDRelativeUri format: {invalidUri} (Parameter 'value')", exception.Message);
    }

    [Theory]
    [InlineData("#example")]
    [InlineData("#anotherExample")]
    public void Equals_SameValue_ReturnsTrue(string uriValue)
    {
        var uri1 = DIDRelativeUri.Parse(uriValue);
        var uri2 = DIDRelativeUri.Parse(uriValue);
        bool result = uri1.Equals(uri2);
        Assert.True(result);
    }

    [Theory]
    [InlineData("#example1", "#example2")]
    public void Equals_DifferentValue_ReturnsFalse(string uriValue1, string uriValue2)
    {
        var uri1 = DIDRelativeUri.Parse(uriValue1);
        var uri2 = DIDRelativeUri.Parse(uriValue2);
        bool result = uri1.Equals(uri2);
        Assert.False(result);
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var uri = DIDRelativeUri.Parse("#example");
        bool result = uri.Equals(null);
        Assert.False(result);
    }

    [Theory]
    [InlineData("#example")]
    public void GetHashCode_SameValue_ReturnsSameHash(string uriValue)
    {
        var uri1 = DIDRelativeUri.Parse(uriValue);
        var uri2 = DIDRelativeUri.Parse(uriValue);
        int hash1 = uri1.GetHashCode();
        int hash2 = uri2.GetHashCode();
        Assert.Equal(hash1, hash2);
    }

    [Theory]
    [InlineData("#example")]
    public void ToString_ReturnsValue(string uriValue)
    {
        var didRelativeUri = DIDRelativeUri.Parse(uriValue);
        string result = didRelativeUri.ToString();
        Assert.Equal(uriValue, result);
    }

    [Theory]
    [InlineData("#example")]
    public void EqualityOperator_SameValue_ReturnsTrue(string uriValue)
    {
        var uri1 = DIDRelativeUri.Parse(uriValue);
        var uri2 = DIDRelativeUri.Parse(uriValue);
        bool result = uri1 == uri2;
        Assert.True(result);
    }

    [Theory]
    [InlineData("#example1", "#example2")]
    public void InequalityOperator_DifferentValue_ReturnsTrue(string uriValue1, string uriValue2)
    {
        var uri1 = DIDRelativeUri.Parse(uriValue1);
        var uri2 = DIDRelativeUri.Parse(uriValue2);
        bool result = uri1 != uri2;
        Assert.True(result);
    }

    [Theory]
    [InlineData("#validUri", true)]
    [InlineData("invalidUri", false)]
    [InlineData("  ", false)]
    public void IsValidDIDRelativeUri_ReturnsExpectedResult(string uriValue, bool expected)
    {
        bool result = DIDRelativeUri.IsValidDIDRelativeUri(uriValue);
        Assert.Equal(expected, result);
    }
}
