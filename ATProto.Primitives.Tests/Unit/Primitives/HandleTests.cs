using ATProto.Primitives.Exceptions;

namespace ATProto.Tests.Primitives;

public class HandleTests
{
    [Theory]
    [InlineData("user.bsky.social")]
    [InlineData("alice.test.com")]
    [InlineData("bob-smith.example.net")]
    [InlineData("a.b.c")]
    public void Create_ValidHandles_CreatesInstance(string handle)
    {
        var result = Handle.Create(handle);

        Assert.Equal(handle, result.Value);
    }

    [Theory]
    [InlineData("user.bsky.social", "USER.BSKY.SOCIAL")]
    [InlineData("test.com", "TEST.COM")]
    [InlineData("mixed-case.example.com", "MIXED-CASE.EXAMPLE.COM")]
    public void Create_MixedCaseHandles_NormalizesToLowerCase(string expected, string input)
    {
        var result = Handle.Create(input);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("user@bsky.social")] // Invalid character @
    [InlineData("user_bsky.social")] // Invalid character _
    [InlineData("user!bsky.social")] // Invalid character !
    [InlineData("user.@com")]
    [InlineData("user space.social")] // Space character
    public void Create_InvalidCharacters_ThrowsInvalidHandleException(string handle)
    {
        var exception = Assert.Throws<InvalidHandleException>(() => Handle.Create(handle));
        Assert.Contains("Disallowed characters", exception.Message);
    }

    [Fact]
    public void Create_TooLongHandle_ThrowsInvalidHandleException()
    {
        var handle = "a".PadRight(254, 'a') + ".com";

        var exception = Assert.Throws<InvalidHandleException>(() => Handle.Create(handle));
        Assert.Contains("Handle is too long", exception.Message);
    }

    [Theory]
    [InlineData("singlelabel")]
    [InlineData("no-dots")]
    public void Create_SingleLabel_ThrowsInvalidHandleException(string handle)
    {
        var exception = Assert.Throws<InvalidHandleException>(() => Handle.Create(handle));
        Assert.Contains("Handle must have at least two parts", exception.Message);
    }

    [Theory]
    [InlineData(".example.com")] // Empty first part
    [InlineData("user..com")] // Empty middle part
    [InlineData("user.example.")] // Empty last part
    public void Create_EmptyLabels_ThrowsInvalidHandleException(string handle)
    {
        var exception = Assert.Throws<InvalidHandleException>(() => Handle.Create(handle));
        Assert.Contains("Handle parts can not be empty", exception.Message);
    }

    [Fact]
    public void Create_TooLongLabel_ThrowsInvalidHandleException()
    {
        var handle = "a".PadRight(64, 'a') + ".com";
        var exception = Assert.Throws<InvalidHandleException>(() => Handle.Create(handle));
        Assert.Equal("Handle part too long (max 63 chars)", exception.Message);
    }

    [Theory]
    [InlineData("-user.example.com")] // Starts with hyphen
    [InlineData("user-.example.com")] // Ends with hyphen
    [InlineData("user.-example.com")] // Part starts with hyphen
    [InlineData("user.example-.com")] // Part ends with hyphen
    public void Create_HyphenAtEdges_ThrowsInvalidHandleException(string handle)
    {
        var exception = Assert.Throws<InvalidHandleException>(() => Handle.Create(handle));
        Assert.Equal("Handle parts can not start or end with hyphens", exception.Message);
    }

    [Theory]
    [InlineData("user.1com")]
    [InlineData("user.2example")]
    public void Create_InvalidTLD_ThrowsInvalidHandleException(string handle)
    {
        var exception = Assert.Throws<InvalidHandleException>(() => Handle.Create(handle));
        Assert.Equal("Handle final component (TLD) must start with ASCII letter", exception.Message);
    }

    [Fact]
    public void IsResolvedHandle_ValidDID_ReturnsTrue()
    {
        var didString = "did:plc:7iza6de2dwap2sbkpav7c6c6abcdef";

        var result = Handle.IsResolvedHandle(didString, out var resolvedHandle);

        Assert.True(result);
        Assert.NotNull(resolvedHandle);
        Assert.Equal(didString, resolvedHandle.Value);
    }

    [Fact]
    public void IsResolvedHandle_InvalidDID_ReturnsFalse()
    {
        var invalidDid = "not-a-did";

        var result = Handle.IsResolvedHandle(invalidDid, out var resolvedHandle);

        Assert.False(result);
        Assert.Null(resolvedHandle);
    }

    [Fact]
    public void Equals_SameHandle_ReturnsTrue()
    {
        var handle1 = Handle.Create("user.example.com");
        var handle2 = Handle.Create("user.example.com");

        Assert.True(handle1.Equals(handle2));
        Assert.True(handle1 == handle2);
    }

    [Fact]
    public void Equals_DifferentHandle_ReturnsFalse()
    {
        var handle1 = Handle.Create("user1.example.com");
        var handle2 = Handle.Create("user2.example.com");

        Assert.False(handle1.Equals(handle2));
        Assert.True(handle1 != handle2);
    }

    [Fact]
    public void GetHashCode_SameHandle_ReturnsSameHash()
    {
        var handle1 = Handle.Create("user.example.com");
        var handle2 = Handle.Create("user.example.com");

        Assert.Equal(handle1.GetHashCode(), handle2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var handleString = "user.example.com";
        var handle = Handle.Create(handleString);

        Assert.Equal(handleString, handle.ToString());
    }
}
