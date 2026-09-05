using ATProto.Primitives;
using ATProto.Primitives.Exceptions;

namespace ATProto.Tests.Primitives;

public class DIDTests
{
    [Theory]
    // Valid DIDs for use in atproto (correct syntax, and supported method)
    [InlineData("did:plc:z72i7hdynmk6r22z27h6tvur")]
    [InlineData("did:web:blueskyweb.xyz")]
    public void Create_ValidDid_ShouldReturnDIDInstance(string input)
    {
        var did = DID.Create(input);
        Assert.NotNull(did);
        Assert.Equal(input, did.Value);
    }

    [Theory]
    [InlineData("invalid:example:123")]
    // Valid DID syntax (would pass Lexicon syntax validation), but unsupported DID method
    [InlineData("did:method:val:two")]
    [InlineData("did:m:v")]
    [InlineData("did:method::::val")]
    [InlineData("did:method:-:_:.")]
    [InlineData("did:key:zQ3shZc2QzApp2oymGvQbzP8eKheVshBHbU4ZYjeXqwSKEn6N")]
    // Invalid DID identitifier syntax (regardless of DID method)
    [InlineData("did:METHOD:val")]
    [InlineData("did:m123:val")]
    [InlineData("DID:method:val")]
    [InlineData("did:method:")]
    [InlineData("did:method:val/two")]
    [InlineData("did:method:val?two")]
    [InlineData("did:method:val#two")]
    public void Create_InvalidDid_ShouldThrowArgumentException(string input)
    {
        var exception = Assert.Throws<ArgumentException>(() => DID.Create(input));
        Assert.Equal($"Invalid DID format: {input} (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void Equality_EqualDIDs_ShouldBeEqual()
    {
        // Arrange
        var did1 = DID.Create("did:plc:z72i7hdynmk6r22z27h6tvur");
        var did2 = DID.Create("did:plc:z72i7hdynmk6r22z27h6tvur");

        // Act & Assert
        Assert.True(did1 == did2);
        Assert.True(did1.Equals(did2));
        Assert.Equal(did1.GetHashCode(), did2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentDIDs_ShouldNotBeEqual()
    {
        // Arrange
        var did1 = DID.Create("did:plc:z72i7hdynmk6r22z27h6tvur");
        var did2 = DID.Create("did:web:blueskyweb.xyz");

        // Act & Assert
        Assert.False(did1 == did2);
        Assert.False(did1.Equals(did2));
        Assert.NotEqual(did1.GetHashCode(), did2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnDIDValue()
    {
        // Arrange
        var did = DID.Create("did:web:blueskyweb.xyz");

        // Act
        string didString = did.ToString();

        // Assert
        Assert.Equal("did:web:blueskyweb.xyz", didString);
    }

    [Theory]
    [InlineData("did:plc:7iza6de2dwap2sbkpav7c6c6")] // Valid PLC DID
    [InlineData("did:plc:aaaaaaaaaaaaaaaaaaaaaaaa")] // Valid with repeating chars
    [InlineData("did:plc:234567234567234567234567")] // Valid with mixed numbers
    public void AssertValidPlc_ValidDIDs_DoesNotThrow(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Record.Exception(() => did.AssertValidPlc());
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("did:web:example.com", "Invalid did:plc prefix")]
    [InlineData("did:plc:short", "did:plc must be 32 characters long")]
    [InlineData("did:plc:7iza6de2dwap2sbkpav7c6c6abcdefgh", "did:plc must be 32 characters long")]
    public void AssertValidPlc_InvalidFormat_ThrowsInvalidDIDException(
        string didValue,
        string expectedMessage
    )
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDException>(() => did.AssertValidPlc());
        Assert.Equal(didValue, exception.DID.Value);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("did:plc:7iza6de2dwap2sbkpav7c6c6abcde!")] // Invalid special char
    [InlineData("did:plc:ABCDEFGHIJKLMNOPQRSTUVWXYZABCD")] // Invalid uppercase
    [InlineData("did:plc:7iza6de2dwap2sbkpav7c6c6abcd8")] // Invalid number (>7)
    [InlineData("did:plc:7iza6de2dwap2sbkpav7c6c6abcd1")] // Invalid number (<2)
    [InlineData("did:plc:7iza6de2dw@p2sbkpav7c6c6abcde")] // Invalid special char
    public void AssertValidPlc_InvalidCharacters_ThrowsInvalidDIDException(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        Assert.Throws<InvalidDIDException>(() => did.AssertValidPlc());
    }

    [Theory]
    [InlineData("did:plc:222222222222222222222222")] // All 2s
    [InlineData("did:plc:777777777777777777777777")] // All 7s
    [InlineData("did:plc:aaaaaaaaaaaaaaaaaaaaaaaa")] // All as
    [InlineData("did:plc:zzzzzzzzzzzzzzzzzzzzzzzz")] // All zs
    public void AssertValidPlc_EdgeCaseCharacters_DoesNotThrow(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Record.Exception(() => did.AssertValidPlc());
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("did:web:example.com")] // Basic valid
    [InlineData("did:web:example-123.com")] // With hyphen and numbers
    [InlineData("did:web:example_123.com")] // With underscore
    [InlineData("did:web:sub.example.com")] // With dots
    [InlineData("did:web:Example.Com")] // Mixed case
    [InlineData("did:web:example%20space")] // Valid percent encoding for space
    [InlineData("did:web:example%2Fslash")] // Valid percent encoding for slash
    [InlineData("did:web:example%41code")] // Valid percent encoding for 'A'
    public void AssertMsid_ValidFormats_DoesNotThrow(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Record.Exception(() => did.AssertMsid());
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("did:web:example.com:")] // Ends with colon
    public void AssertMsid_EndsWithColon_ThrowsInvalidDIDException(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDException>(() => did.AssertMsid());
        Assert.Equal("DID cannot end with a \":\"", exception.Message);
    }

    [Theory]
    [InlineData("did:web:test%")] // Single % at end
    [InlineData("did:web:test%2")] // Incomplete percent encoding
    public void AssertMsid_IncompletePctEncoding_ThrowsInvalidDIDException(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDException>(() => did.AssertMsid());
        Assert.Contains("Incomplete pct-encoded character", exception.Message);
    }

    [Theory]
    [InlineData("did:web:test%GG")] // Invalid hex digits
    [InlineData("did:web:test%ZZ")] // Invalid hex digits
    [InlineData("did:web:test%2G")] // Second digit invalid
    [InlineData("did:web:test%G2")] // First digit invalid
    public void AssertMsid_InvalidPctEncoding_ThrowsInvalidDIDException(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDException>(() => did.AssertMsid());
        Assert.Contains("Invalid pct-encoded character", exception.Message);
    }

    [Theory]
    [InlineData("did:web:example!com")] // Exclamation mark
    [InlineData("did:web:example#com")] // Hash
    [InlineData("did:web:example@com")] // At symbol
    [InlineData("did:web:example$com")] // Dollar sign
    [InlineData("did:web:example&com")] // Ampersand
    public void AssertMsid_DisallowedCharacters_ThrowsInvalidDIDException(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDException>(() => did.AssertMsid());
        Assert.Contains("Disallowed character in DID at position", exception.Message);
    }

    [Theory]
    [InlineData("did:web:example.com", 8)] // Start after "did:web:"
    [InlineData("did:web:example.com", 12)] // Start in middle
    public void AssertMsid_WithCustomStartPosition_ValidatesCorrectly(string didValue, int startPos)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Record.Exception(() => did.AssertMsid(startPos));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("did:web:test%20%2F%41")] // Multiple valid percent encodings
    [InlineData("did:web:test%20test%20test")] // Multiple spaces
    [InlineData("did:web:test%2Ftest%2Ftest")] // Mixed case hex
    public void AssertMsid_MultipleValidPctEncodings_DoesNotThrow(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Record.Exception(() => did.AssertMsid());
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("did:web:test%%20")] // Invalid percent sequence
    [InlineData("did:web:test%2F%")] // Valid followed by invalid
    [InlineData("did:web:test%2F%G")] // Valid followed by invalid
    public void AssertMsid_MixedValidAndInvalidPctEncoding_ThrowsInvalidDIDException(string didValue)
    {
        // Arrange
        var did = DID.Create(didValue);

        // Act & Assert
        var exception = Assert.Throws<InvalidDIDException>(() => did.AssertMsid());
        Assert.True(
            exception.Message.Contains("Invalid pct-encoded character") ||
            exception.Message.Contains("Incomplete pct-encoded character")
        );
    }
}
