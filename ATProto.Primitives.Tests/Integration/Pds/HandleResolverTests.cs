using ATProto.Identity;
using ATProto.Primitives;

namespace ATProto.Tests.Integration.Pds;

/// <summary>
/// Integration tests for HandleResolver functionality using a test PDS instance.
/// Tests handle-to-DID resolution against a real PDS.
/// </summary>
[Collection("PDS")]
public class HandleResolverTests(PdsTestContainer pds)
{
    private readonly PdsTestContainer _pds = pds;
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    [Fact]
    public async Task ResolveAsync_WithValidHandle_ResolvesToDID()
    {
        // Arrange
        var handle = $"alice{Guid.NewGuid().ToString("N")[..8]}.test";
        var createdDid = await _pds.CreateAccountAsync(handle, $"alice{Guid.NewGuid().ToString("N")[..8]}@example.com", "password123", TestContext.Current.CancellationToken);

        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var handleObj = Handle.Create(handle);

        // Act
        var resolvedDid = await handleResolver.ResolveAsync(
            handleObj,
            noCache: true,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(resolvedDid);
        Assert.Equal(createdDid, resolvedDid.ToString());
    }

    [Fact]
    public async Task ResolveAsync_WithMultipleHandles_ResolvesIndependently()
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

        // Act & Assert: Resolve each account independently
        foreach (var (handle, expectedDid) in accounts)
        {
            var handleObj = Handle.Create(handle);
            var resolvedDid = await handleResolver.ResolveAsync(
                handleObj,
                noCache: true,
                TestContext.Current.CancellationToken);

            Assert.NotNull(resolvedDid);
            Assert.Equal(expectedDid, resolvedDid.ToString());
        }
    }

    [Fact]
    public async Task ResolveAsync_WithCustomPdsUrl_ResolvesThroughSpecificInstance()
    {
        // This test demonstrates how the HandleResolver can be configured
        // to resolve handles through a specific PDS instance rather than
        // using the default bsky.social

        // Arrange: Create account
        var handle = $"custom-{Guid.NewGuid().ToString("N")[..8]}.test";
        var expectedDid = await _pds.CreateAccountAsync(handle, $"custom-{Guid.NewGuid().ToString("N")[..8]}@example.com", "password123", TestContext.Current.CancellationToken);

        // Create resolver pointing to our test PDS
        var handleResolver = new HandleResolver(
            _httpClient,
            new Uri(_pds.PdsUrl)); // Custom PDS URL

        // Act
        var resolvedDid = await handleResolver.ResolveAsync(
            Handle.Create(handle),
            noCache: true,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(resolvedDid);
        Assert.Equal(expectedDid, resolvedDid.ToString());
    }

    [Fact]
    public async Task ResolveAsync_WithNonExistentHandle_ReturnsNull()
    {
        // Arrange
        var handleResolver = new HandleResolver(_httpClient, new Uri(_pds.PdsUrl));
        var nonExistentHandle = Handle.Create("nonexistent.test");

        // Act
        var result = await handleResolver.ResolveAsync(
            nonExistentHandle,
            noCache: true,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }
}
