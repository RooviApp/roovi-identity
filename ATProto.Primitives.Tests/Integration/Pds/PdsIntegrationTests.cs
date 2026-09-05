using ATProto.Identity;

namespace ATProto.Tests.Integration.Pds;

/// <summary>
/// Example integration tests using a real PDS instance via Testcontainers.
/// Each test class gets its own isolated PDS instance that is automatically
/// started before tests run and cleaned up afterwards.
/// </summary>
[Collection("PDS")]
public class PdsIntegrationTests(PdsTestContainer pds)
{
    private readonly PdsTestContainer _pds = pds;
    private readonly HttpClient _httpClient = new();

  [Fact]
    public async Task PdsHealthEndpoint_ShouldReturnVersion()
    {
        // Arrange
        var healthUrl = $"{_pds.PdsUrl}/xrpc/_health";

        // Act
        var response = await _httpClient.GetAsync(healthUrl, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsSuccessStatusCode,
            $"Health check failed. URL: {healthUrl}");

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("version", content);
    }

    [Fact]
    public async Task CreateAccount_ShouldSucceed()
    {
        // Arrange
        var handle = $"testuser{Guid.NewGuid().ToString("N")[..8]}.test";
        var email = $"test{Guid.NewGuid().ToString("N")[..8]}@example.com";
        var password = "TestPassword123!";

        // Act
        var did = await _pds.CreateAccountAsync(handle, email, password, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(did);
        Assert.StartsWith("did:plc:", did);
    }

    [Fact]
    public async Task ResolveIdentity_WithTestPdsAccount_ShouldReturnValidDid()
    {
        // Arrange
        var handle = $"testuser{Guid.NewGuid().ToString("N")[..8]}.test";
        var email = $"test{Guid.NewGuid().ToString("N")[..8]}@example.com";
        var password = "TestPassword123!";

        var createdDid = await _pds.CreateAccountAsync(handle, email, password, TestContext.Current.CancellationToken);

        // Create identity resolver components
        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var didResolver = new DIDResolver(_httpClient);
        var identityResolver = new IdentityResolver(handleResolver, didResolver);

        // Act
        var result = await identityResolver.GetDIDDocument(
            input: handle,
            noCache: true,
            plcDirectoryUrl: _pds.PlcUrl,
            allowHttp: true, // Allow HTTP since our test PDS doesn't use HTTPS
            ct: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.DID);
        Assert.Equal(createdDid, result.DID.ToString());
        Assert.NotNull(result.Pds);

        // The PDS URL should match our test instance
        Assert.Contains(_pds.ServiceUrl.TrimEnd('/'), result.Pds.AbsoluteUri);
    }

    [Fact]
    public async Task CreateInviteCode_ShouldReturnValidCode()
    {
        // Act
        var inviteCode = await _pds.CreateInviteCodeAsync(TestContext.Current.CancellationToken);

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
            var handle = $"user{i}-{Guid.NewGuid().ToString("N")[..8]}.test";
            var email = $"user{i}-{Guid.NewGuid().ToString("N")[..8]}@example.com";
            var did = await _pds.CreateAccountAsync(handle, email, "password123", TestContext.Current.CancellationToken);
            accounts.Add(did);
        }

        // Assert
        Assert.Equal(3, accounts.Count);
        Assert.All(accounts, did => Assert.StartsWith("did:plc:", did));
        // All DIDs should be unique
        Assert.Equal(accounts.Count, accounts.Distinct().Count());
    }
}
