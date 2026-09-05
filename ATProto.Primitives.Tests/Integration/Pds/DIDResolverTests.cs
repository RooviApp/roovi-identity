using ATProto.Identity;
using ATProto.Primitives;

namespace ATProto.Tests.Integration.Pds;

/// <summary>
/// Integration tests for DIDResolver functionality using a test PDS instance.
/// Tests DID document resolution from the PLC directory for test accounts.
/// </summary>
public class DIDResolverTests(PdsTestContainer pds) : IClassFixture<PdsTestContainer>
{
    private readonly PdsTestContainer _pds = pds;
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    [Fact]
    public async Task ResolveAsync_WithValidDID_ResolvesDIDDocument()
    {
        // Arrange: Create account and get DID
        var handle = $"bob{Guid.NewGuid():N}.test";
        var createdDid = await _pds.CreateAccountAsync(
            handle,
            $"bob{Guid.NewGuid():N}@example.com",
            "password123");

        var didObj = DID.Create(createdDid);
        var didResolver = new DIDResolver(_httpClient);

        // Act: Resolve the DID document from PLC directory
        var document = await didResolver.ResolveAsync(
            didObj,
            noCache: true,
            plcDirectoryUrl: "https://plc.directory/",
            allowHttp: true, // Allow HTTP for test PDS
            CancellationToken.None);

        // Assert: Verify DID document structure
        Assert.NotNull(document);
        Assert.Equal(createdDid, document.Id?.ToString());
        Assert.NotNull(document.Service);
        Assert.NotEmpty(document.Service);
    }

    [Fact]
    public async Task ResolveAsync_DIDDocument_ContainsAtprotoPersonalDataServerService()
    {
        // Arrange
        var handle = $"carol{Guid.NewGuid():N}.test";
        var createdDid = await _pds.CreateAccountAsync(
            handle,
            $"carol{Guid.NewGuid():N}@example.com",
            "password123");

        var didObj = DID.Create(createdDid);
        var didResolver = new DIDResolver(_httpClient);

        // Act
        var document = await didResolver.ResolveAsync(
            didObj,
            noCache: true,
            plcDirectoryUrl: "https://plc.directory/",
            allowHttp: true,
            CancellationToken.None);

        // Assert: Should have an AtprotoPersonalDataServer service
        var pdsService = document.Service.FirstOrDefault(
            s => s.Type == "AtprotoPersonalDataServer");
        Assert.NotNull(pdsService);

        // The service endpoint should reference our test PDS
        Assert.Equal(DIDServiceEndpoint.EndpointType.Uri, pdsService.ServiceEndpoint.Type);
        Assert.Contains(_pds.PdsUrl, pdsService.ServiceEndpoint.Uri);
    }

    [Fact]
    public async Task ResolveAsync_WithMultipleDIDs_ResolvesCorrectDocuments()
    {
        // Arrange: Create multiple accounts
        var accounts = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            var did = await _pds.CreateAccountAsync(
                $"user{i}-{Guid.NewGuid():N}.test",
                $"user{i}@example.com",
                "password123");
            accounts.Add(did);
        }

        var didResolver = new DIDResolver(_httpClient);

        // Act & Assert: Resolve each DID
        foreach (var expectedDid in accounts)
        {
            var didObj = DID.Create(expectedDid);
            var document = await didResolver.ResolveAsync(
                didObj,
                noCache: true,
                plcDirectoryUrl: "https://plc.directory/",
                allowHttp: true,
                CancellationToken.None);

            Assert.NotNull(document);
            Assert.Equal(expectedDid, document.Id?.ToString());
        }
    }
}
