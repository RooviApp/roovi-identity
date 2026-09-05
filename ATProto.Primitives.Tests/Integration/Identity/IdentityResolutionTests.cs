using ATProto.Identity;
using ATProto.Tests.Integration.Pds;

namespace ATProto.Tests.Integration.Identity;

[Collection("PDS")]
public class IdentityResolutionTests(PdsTestContainer pds)
{
    [Fact]
    public async Task GetDIDDocument_WithValidHandle_ResolvesCompleteIdentity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handle = $"identity-{Guid.NewGuid().ToString("N")[..8]}.test";
        var did = await pds.CreateAccountAsync(handle, $"{Guid.NewGuid().ToString("N")[..8]}@example.com", "test-password", cancellationToken);
        using var httpClient = new HttpClient();
        var resolver = new IdentityResolver(new HandleResolver(httpClient, new Uri(pds.PdsUrl)), new DIDResolver(httpClient));

        var result = await resolver.GetDIDDocument(handle, true, pds.PlcUrl, true, cancellationToken);

        Assert.Equal(did, result.DID.Value);
        Assert.Equal(pds.ServiceUrl, result.Pds.GetLeftPart(UriPartial.Authority));
    }
}
