using ATProto.Identity;
using ATProto.Primitives;

namespace ATProto.Tests.Integration.Pds;

/// <summary>
/// Advanced integration tests demonstrating how to use the PDS test container
/// with your actual IdentityResolver, HandleResolver, and DIDResolver implementations.
/// </summary>
public class AdvancedIdentityResolverTests : IClassFixture<PdsTestContainer>
{
    private readonly PdsTestContainer _pds;
    private readonly HttpClient _httpClient;

    public AdvancedIdentityResolverTests(PdsTestContainer pds)
    {
        _pds = pds;
        _httpClient = new HttpClient
        {
            // Configure timeout for slower container operations
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    [Fact]
    public async Task IdentityResolver_WithTestPdsAccount_ResolvesCompleteIdentity()
    {
        // Arrange: Create a test account on the PDS
        var handle = $"alice-{Guid.NewGuid():N}.test";
        var email = $"alice-{Guid.NewGuid():N}@example.com";
        var password = "SecurePassword123!";

        var createdDid = await _pds.CreateAccountAsync(handle, email, password);

        // Create identity resolver components pointing to our test PDS
        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var didResolver = new DIDResolver(_httpClient);
        var identityResolver = new IdentityResolver(handleResolver, didResolver);

        // Act: Resolve the identity
        var result = await identityResolver.GetDIDDocument(
            input: handle,
            noCache: true,
            plcDirectoryUrl: "https://plc.directory/",
            allowHttp: true, // IMPORTANT: Test PDS uses HTTP
            ct: CancellationToken.None);

        // Assert: Verify all identity components
        Assert.NotNull(result);
        Assert.NotNull(result.DID);
        Assert.NotNull(result.Pds);

        // The DID from resolution should match what we created
        Assert.Equal(createdDid, result.DID.ToString());

        // The PDS endpoint should point to our test instance
        Assert.Contains(_pds.PdsUrl.TrimEnd('/'), result.Pds.AbsoluteUri);
    }

    [Fact]
    public async Task HandleResolver_WithTestAccount_ResolvesDID()
    {
        // Arrange
        var handle = $"bob-{Guid.NewGuid():N}.test";
        var createdDid = await _pds.CreateAccountAsync(
            handle,
            $"bob-{Guid.NewGuid():N}@example.com",
            "password123");

        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var handleObj = Handle.Create(handle);

        // Act
        var resolvedDid = await handleResolver.ResolveAsync(
            handleObj,
            noCache: true,
            CancellationToken.None);

        // Assert
        Assert.NotNull(resolvedDid);
        Assert.Equal(createdDid, resolvedDid.ToString());
    }

    [Fact]
    public async Task DIDResolver_WithTestAccount_ResolvesDIDDocument()
    {
        // Arrange: Create account and get DID
        var handle = $"carol-{Guid.NewGuid():N}.test";
        var createdDid = await _pds.CreateAccountAsync(
            handle,
            $"carol-{Guid.NewGuid():N}@example.com",
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

        // Should have an AtprotoPersonalDataServer service
        var pdsService = document.Service.FirstOrDefault(
            s => s.Type == "AtprotoPersonalDataServer");
        Assert.NotNull(pdsService);

        // The service endpoint should reference our test PDS
        Assert.Equal(DIDServiceEndpoint.EndpointType.Uri, pdsService.ServiceEndpoint.Type);
        Assert.Contains(_pds.PdsUrl, pdsService.ServiceEndpoint.Uri);
    }

    [Fact]
    public async Task IdentityResolver_WithDIDInput_ResolvesDirectly()
    {
        // Arrange: Create account
        var handle = $"dave-{Guid.NewGuid():N}.test";
        var createdDid = await _pds.CreateAccountAsync(
            handle,
            $"dave-{Guid.NewGuid():N}@example.com",
            "password123");

        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var didResolver = new DIDResolver(_httpClient);
        var identityResolver = new IdentityResolver(handleResolver, didResolver);

        // Act: Resolve using DID instead of handle
        var result = await identityResolver.GetDIDDocument(
            input: createdDid, // Pass DID directly
            noCache: true,
            plcDirectoryUrl: "https://plc.directory/",
            allowHttp: true,
            ct: CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdDid, result.DID.ToString());
        Assert.NotNull(result.Pds);
    }

    [Fact]
    public async Task MultipleAccounts_CanBeResolvedIndependently()
    {
        // Arrange: Create multiple accounts
        var accounts = new List<(string handle, string did)>();

        for (int i = 0; i < 3; i++)
        {
            var handle = $"user{i}-{Guid.NewGuid():N}.test";
            var did = await _pds.CreateAccountAsync(
                handle,
                $"user{i}-{Guid.NewGuid():N}@example.com",
                "password123");

            accounts.Add((handle, did));
        }

        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));

        // Act & Assert: Resolve each account independently
        foreach (var (handle, expectedDid) in accounts)
        {
            var handleObj = Handle.Create(handle);
            var resolvedDid = await handleResolver.ResolveAsync(
                handleObj,
                noCache: true,
                CancellationToken.None);

            Assert.NotNull(resolvedDid);
            Assert.Equal(expectedDid, resolvedDid.ToString());
        }
    }

    [Fact]
    public async Task IdentityResolver_WithInvalidHandle_ThrowsException()
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
                plcDirectoryUrl: "https://plc.directory/",
                allowHttp: true,
                ct: CancellationToken.None);
        });
    }

    [Fact]
    public async Task HandleResolver_WithCustomPdsUrl_ResolvesThroughSpecificInstance()
    {
        // This test demonstrates how the HandleResolver can be configured
        // to resolve handles through a specific PDS instance rather than
        // using the default bsky.social

        // Arrange: Create account
        var handle = $"custom-{Guid.NewGuid():N}.test";
        var expectedDid = await _pds.CreateAccountAsync(
            handle,
            $"custom-{Guid.NewGuid():N}@example.com",
            "password123");

        // Create resolver pointing to our test PDS
        var handleResolver = new HandleResolver(
            _httpClient,
            new Uri(_pds.PdsUrl)); // Custom PDS URL

        // Act
        var resolvedDid = await handleResolver.ResolveAsync(
            Handle.Create(handle),
            noCache: true,
            CancellationToken.None);

        // Assert
        Assert.NotNull(resolvedDid);
        Assert.Equal(expectedDid, resolvedDid.ToString());
    }

    [Fact]
    public async Task EndToEnd_CreateAccountAndResolveIdentity()
    {
        // This test demonstrates a complete end-to-end workflow:
        // 1. Create a new account on the PDS
        // 2. Resolve the handle to a DID
        // 3. Resolve the DID to get the DID document
        // 4. Extract the PDS endpoint from the document

        // Step 1: Create account
        var handle = $"endtoend-{Guid.NewGuid():N}.test";
        var email = $"endtoend-{Guid.NewGuid():N}@example.com";
        var password = "TestPassword123!";

        var createdDid = await _pds.CreateAccountAsync(handle, email, password);

        // Step 2: Resolve handle to DID
        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var handleObj = Handle.Create(handle);
        var resolvedDid = await handleResolver.ResolveAsync(handleObj, true, CancellationToken.None);

        Assert.NotNull(resolvedDid);
        Assert.Equal(createdDid, resolvedDid.ToString());

        // Step 3: Resolve DID to DID document
        var didResolver = new DIDResolver(_httpClient);
        var didDocument = await didResolver.ResolveAsync(
            resolvedDid!,
            noCache: true,
            plcDirectoryUrl: "https://plc.directory/",
            allowHttp: true,
            CancellationToken.None);

        Assert.NotNull(didDocument);
        Assert.Equal(createdDid, didDocument.Id?.ToString());

        // Step 4: Extract PDS endpoint
        var pdsService = didDocument.Service.FirstOrDefault(
            s => s.Type == "AtprotoPersonalDataServer");

        Assert.NotNull(pdsService);
        Assert.Equal(DIDServiceEndpoint.EndpointType.Uri, pdsService.ServiceEndpoint.Type);

        var pdsEndpoint = pdsService.ServiceEndpoint.Uri;
        Assert.Contains(_pds.PdsUrl, pdsEndpoint);
    }
}
