namespace ATProto.Tests.Integration.Pds;

/// <summary>
/// Tests for basic PDS container functionality including health checks,
/// account creation, and administrative operations.
/// </summary>
[Collection("PDS")]
public class PdsContainerTests(PdsTestContainer pds)
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

    [Fact]
    public void PdsUrl_ShouldBeAccessible()
    {
        // Arrange & Act
        var url = _pds.PdsUrl;

        // Assert
        Assert.NotNull(url);
        Assert.NotEmpty(url);
        Assert.StartsWith("http://", url);
    }

    [Fact]
    public void AdminPassword_ShouldBeAvailable()
    {
        // Act
        var adminPassword = _pds.AdminPassword;

        // Assert
        Assert.NotNull(adminPassword);
        Assert.NotEmpty(adminPassword);
    }
}
