using ATProto.Identity;
using ATProto.Primitives;

namespace ATProto.Tests.Integration.Pds;

/// <summary>
/// Integration tests for IdentityResolver end-to-end workflows.
/// Tests complete identity resolution from handle/DID to PDS endpoint discovery.
/// </summary>
[Collection("PDS")]
public class IdentityResolverTests(PdsTestContainer pds)
{
    private readonly PdsTestContainer _pds = pds;
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    [Fact]
    public async Task GetDIDDocument_WithHandle_ResolvesCompleteIdentity()
    {
        // Arrange: Create a test account on the PDS
        var handle = $"alice-{Guid.NewGuid().ToString("N")[..8]}.test";
        var email = $"alice-{Guid.NewGuid().ToString("N")[..8]}@example.com";
        var password = "SecurePassword123!";

        var createdDid = await _pds.CreateAccountAsync(handle, email, password, TestContext.Current.CancellationToken);

        // Create identity resolver components pointing to our test PDS
        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var didResolver = new DIDResolver(_httpClient);
        var identityResolver = new IdentityResolver(handleResolver, didResolver);

        // Act: Resolve the identity
        var result = await identityResolver.GetDIDDocument(
            input: handle,
            noCache: true,
            plcDirectoryUrl: _pds.PlcUrl,
            allowHttp: true, // IMPORTANT: Test PDS uses HTTP
            ct: TestContext.Current.CancellationToken);

        // Assert: Verify all identity components
        Assert.NotNull(result);
        Assert.NotNull(result.DID);
        Assert.NotNull(result.Pds);

        // The DID from resolution should match what we created
        Assert.Equal(createdDid, result.DID.ToString());

        // The PDS endpoint should point to our test instance
        Assert.Contains(_pds.ServiceUrl.TrimEnd('/'), result.Pds.AbsoluteUri);
    }

    [Fact]
    public async Task GetDIDDocument_WithDID_ResolvesDirectly()
    {
        // Arrange: Create account
        var handle = $"bob-{Guid.NewGuid().ToString("N")[..8]}.test";
        var createdDid = await _pds.CreateAccountAsync(handle, $"bob-{Guid.NewGuid().ToString("N")[..8]}@example.com", "password123", TestContext.Current.CancellationToken);

        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var didResolver = new DIDResolver(_httpClient);
        var identityResolver = new IdentityResolver(handleResolver, didResolver);

        // Act: Resolve using DID instead of handle
        var result = await identityResolver.GetDIDDocument(
            input: createdDid, // Pass DID directly
            noCache: true,
            plcDirectoryUrl: _pds.PlcUrl,
            allowHttp: true,
            ct: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdDid, result.DID.ToString());
        Assert.NotNull(result.Pds);
    }

    [Fact]
    public async Task GetDIDDocument_WithInvalidHandle_ThrowsException()
    {
        // Arrange
        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var didResolver = new DIDResolver(_httpClient);
        var identityResolver = new IdentityResolver(handleResolver, didResolver);

        var nonExistentHandle = "nonexistent.test";

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await identityResolver.GetDIDDocument(
                input: nonExistentHandle,
                noCache: true,
                plcDirectoryUrl: _pds.PlcUrl,
                allowHttp: true,
                ct: TestContext.Current.CancellationToken);
        });
    }

    [Fact]
    public async Task GetDIDDocument_EndToEnd_CompleteWorkflow()
    {
        // This test demonstrates a complete end-to-end workflow:
        // 1. Create a new account on the PDS
        // 2. Resolve the handle to a DID
        // 3. Resolve the DID to get the DID document
        // 4. Extract the PDS endpoint from the document

        // Step 1: Create account
        var handle = $"endtoend-{Guid.NewGuid().ToString("N")[..8]}.test";
        var email = $"endtoend-{Guid.NewGuid().ToString("N")[..8]}@example.com";
        var password = "TestPassword123!";

        var createdDid = await _pds.CreateAccountAsync(handle, email, password, TestContext.Current.CancellationToken);

        // Step 2: Resolve handle to DID
        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var handleObj = Handle.Create(handle);
        var resolvedDid = await handleResolver.ResolveAsync(handleObj, true, TestContext.Current.CancellationToken);

        Assert.NotNull(resolvedDid);
        Assert.Equal(createdDid, resolvedDid.ToString());

        // Step 3: Resolve DID to DID document
        var didResolver = new DIDResolver(_httpClient);
        var didDocument = await didResolver.ResolveAsync(
            resolvedDid!,
            noCache: true,
            plcDirectoryUrl: _pds.PlcUrl,
            allowHttp: true,
            TestContext.Current.CancellationToken);

        Assert.NotNull(didDocument);
        Assert.Equal(createdDid, didDocument.Id?.ToString());

        // Step 4: Extract PDS endpoint
        var pdsService = didDocument.Service.FirstOrDefault(
            s => s.Type == "AtprotoPersonalDataServer");

        Assert.NotNull(pdsService);
        Assert.Equal(DIDServiceEndpoint.EndpointType.Uri, pdsService.ServiceEndpoint.Type);

        var pdsEndpoint = pdsService.ServiceEndpoint.Uri;
        Assert.Contains(_pds.ServiceUrl, pdsEndpoint);
    }

    [Fact]
    public async Task GetDIDDocument_WithMultipleAccounts_ResolvesCorrectly()
    {
        // Arrange: Create multiple accounts
        var accounts = new List<(string handle, string did)>();

        for (int i = 0; i < 3; i++)
        {
            var handle = $"user{i}-{Guid.NewGuid().ToString("N")[..8]}.test";
            var did = await _pds.CreateAccountAsync(handle, $"user{i}-{Guid.NewGuid().ToString("N")[..8]}@example.com", "password123", TestContext.Current.CancellationToken);

            accounts.Add((handle, did));
        }

        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var didResolver = new DIDResolver(_httpClient);
        var identityResolver = new IdentityResolver(handleResolver, didResolver);

        // Act & Assert: Resolve each account's complete identity
        foreach (var (handle, expectedDid) in accounts)
        {
            var result = await identityResolver.GetDIDDocument(
                input: handle,
                noCache: true,
                plcDirectoryUrl: _pds.PlcUrl,
                allowHttp: true,
                ct: TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(expectedDid, result.DID.ToString());
            Assert.Contains(_pds.ServiceUrl, result.Pds.AbsoluteUri);
        }
    }
}
