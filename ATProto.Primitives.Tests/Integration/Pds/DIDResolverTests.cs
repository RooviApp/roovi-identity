using ATProto.Identity;
using ATProto.Primitives;

namespace ATProto.Tests.Integration.Pds;

/// <summary>
/// Integration tests for DIDResolver functionality using a test PDS instance.
/// Tests DID document resolution from the PLC directory for test accounts.
/// </summary>
[Collection("PDS")]
public class DIDResolverTests(PdsTestContainer pds)
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
        var handle = $"bob{Guid.NewGuid().ToString("N")[..8]}.test";
        var createdDid = await _pds.CreateAccountAsync(handle, $"bob{Guid.NewGuid().ToString("N")[..8]}@example.com", "password123", TestContext.Current.CancellationToken);

        var didObj = DID.Create(createdDid);
        var didResolver = new DIDResolver(_httpClient);

        // Act: Resolve the DID document from PLC directory
        var document = await didResolver.ResolveAsync(
            didObj,
            noCache: true,
            plcDirectoryUrl: _pds.PlcUrl,
            allowHttp: true, // Allow HTTP for test PDS
            TestContext.Current.CancellationToken);

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
        var handle = $"carol{Guid.NewGuid().ToString("N")[..8]}.test";
        var createdDid = await _pds.CreateAccountAsync(handle, $"carol{Guid.NewGuid().ToString("N")[..8]}@example.com", "password123", TestContext.Current.CancellationToken);

        var didObj = DID.Create(createdDid);
        var didResolver = new DIDResolver(_httpClient);

        // Act
        var document = await didResolver.ResolveAsync(
            didObj,
            noCache: true,
            plcDirectoryUrl: _pds.PlcUrl,
            allowHttp: true,
            TestContext.Current.CancellationToken);

        // Assert: Should have an AtprotoPersonalDataServer service
        var pdsService = document.Service.FirstOrDefault(
            s => s.Type == "AtprotoPersonalDataServer");
        Assert.NotNull(pdsService);

        // The service endpoint should reference our test PDS
        Assert.Equal(DIDServiceEndpoint.EndpointType.Uri, pdsService.ServiceEndpoint.Type);
        Assert.Contains(_pds.ServiceUrl, pdsService.ServiceEndpoint.Uri);
    }

    [Fact]
    public async Task ResolveAsync_WithMultipleDIDs_ResolvesCorrectDocuments()
    {
        // Arrange: Create multiple accounts
        var accounts = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            var did = await _pds.CreateAccountAsync($"user{i}-{Guid.NewGuid().ToString("N")[..8]}.test", $"user{i}@example.com", "password123", TestContext.Current.CancellationToken);
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
                plcDirectoryUrl: _pds.PlcUrl,
                allowHttp: true,
                TestContext.Current.CancellationToken);

            Assert.NotNull(document);
            Assert.Equal(expectedDid, document.Id?.ToString());
        }
    }
}
