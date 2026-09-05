using ATProto.Identity;

namespace ATProto.Tests.Integration.Pds;

/// <summary>
/// Example integration tests using a real PDS instance via Testcontainers.
/// Each test class gets its own isolated PDS instance that is automatically
/// started before tests run and cleaned up afterwards.
/// </summary>
public class PdsIntegrationTests(PdsTestContainer pds) : IClassFixture<PdsTestContainer>
{
    private readonly PdsTestContainer _pds = pds;
    private readonly HttpClient _httpClient = new();

  [Fact]
    public async Task PdsHealthEndpoint_ShouldReturnVersion()
    {
        // Arrange
        var healthUrl = $"{_pds.PdsUrl}/xrpc/_health";

        // Act
        var response = await _httpClient.GetAsync(healthUrl);

        // Assert
        Assert.True(response.IsSuccessStatusCode,
            $"Health check failed. URL: {healthUrl}");

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("version", content);
    }

    [Fact]
    public async Task CreateAccount_ShouldSucceed()
    {
        // Arrange
        var handle = $"testuser{Guid.NewGuid():N}.test";
        var email = $"test{Guid.NewGuid():N}@example.com";
        var password = "TestPassword123!";

        // Act
        var did = await _pds.CreateAccountAsync(handle, email, password);

        // Assert
        Assert.NotNull(did);
        Assert.StartsWith("did:plc:", did);
    }

    [Fact]
    public async Task ResolveIdentity_WithTestPdsAccount_ShouldReturnValidDid()
    {
        // Arrange
        var handle = $"testuser{Guid.NewGuid():N}.test";
        var email = $"test{Guid.NewGuid():N}@example.com";
        var password = "TestPassword123!";

        var createdDid = await _pds.CreateAccountAsync(handle, email, password);

        // Create identity resolver components
        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var didResolver = new DIDResolver(_httpClient);
        var identityResolver = new IdentityResolver(handleResolver, didResolver);

        // Act
        var result = await identityResolver.GetDIDDocument(
            input: handle,
            noCache: true,
            plcDirectoryUrl: "https://plc.directory/",
            allowHttp: true, // Allow HTTP since our test PDS doesn't use HTTPS
            ct: CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.DID);
        Assert.Equal(createdDid, result.DID.ToString());
        Assert.NotNull(result.Pds);

        // The PDS URL should match our test instance
        Assert.Contains(_pds.PdsUrl.TrimEnd('/'), result.Pds.AbsoluteUri);
    }

    [Fact]
    public async Task CreateInviteCode_ShouldReturnValidCode()
    {
        // Act
        var inviteCode = await _pds.CreateInviteCodeAsync();

        // Assert
        Assert.NotNull(inviteCode);
        Assert.NotEmpty(inviteCode);
    }

    [Fact]
    public async Task MultipleAccounts_CanBeCreatedOnSamePds()
    {
        // Arrange
        var accounts = new List<string>();

        // Act - Create 3 accounts
        for (int i = 0; i < 3; i++)
        {
            var handle = $"user{i}-{Guid.NewGuid():N}.test";
            var email = $"user{i}-{Guid.NewGuid():N}@example.com";
            var did = await _pds.CreateAccountAsync(handle, email, "password123");
            accounts.Add(did);
        }

        // Assert
        Assert.Equal(3, accounts.Count);
        Assert.All(accounts, did => Assert.StartsWith("did:plc:", did));
        // All DIDs should be unique
        Assert.Equal(accounts.Count, accounts.Distinct().Count());
    }
}
