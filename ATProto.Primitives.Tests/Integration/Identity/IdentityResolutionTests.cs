using ATProto.Identity;

namespace ATProto.Tests.Integration.Identity;

/// <summary>
/// Integration tests for identity resolution workflows.
/// These tests verify end-to-end functionality including external service calls.
/// </summary>
public class IdentityResolutionTests
{
    [Fact]
    public async Task GetDIDDocument_WithValidHandle_ResolvesCompleteIdentity()
    {
        // Arrange
        var handleResolver = new HandleResolver(new HttpClient(), new Uri("https://bsky.social"));
        var dIDResolver = new DIDResolver(new HttpClient());
        var identityResolver = new IdentityResolver(handleResolver, dIDResolver);
        var input = "sentienthusk.bsky.social";
        var noCache = true;
        var plcDirectoryUrl = "https://plc.directory/";
        var allowHttp = false;
        var ct = new CancellationToken();

        // Act
        var result = await identityResolver.GetDIDDocument(input, noCache, plcDirectoryUrl, allowHttp, ct);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.DID);
        Assert.NotNull(result.Pds);
        Assert.NotNull(result.Pds.AbsoluteUri);
    }
}
