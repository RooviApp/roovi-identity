using ATProto.Identity;
using ATProto.Tests.TestUtilities;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ATProto.Tests.Identity;

public class HandleResolverTests
{
    private readonly HttpClient _httpClient;
    private readonly Uri _serviceUri = new("https://example.com");

    public HandleResolverTests()
    {
        _httpClient = new HttpClient(new TestHttpMessageHandler());
    }

    [Fact]
    public async Task ResolveAsync_ValidHandle_ReturnsDID()
    {
        var handle = Handle.Create("test.atproto.app");
        var expectedDid = "did:plc:7iza6de2dwap2sbkpav7c6c6abcdef";
        var response = new ResolveHandleResponse
        {
            Did = expectedDid
        };

        var handler = new TestHttpMessageHandler((request, ct) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("?handle=test.atproto.app", request.RequestUri!.Query);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(response)
            };
        });

        var httpClient = new HttpClient(handler);
        var resolver = new HandleResolver(httpClient, _serviceUri);

        var result = await resolver.ResolveAsync(handle, noCache: false);

        Assert.NotNull(result);
        Assert.Equal(expectedDid, result.Value);
    }

    [Fact]
    public async Task ResolveAsync_UnresolvableHandle_ReturnsNull()
    {
        var handle = Handle.Create("nonexistent.atprot.app");
        var error = new XrpcError
        {
            Error = "InvalidRequest",
            Message = "Unable to resolve handle"
        };

        var handler = new TestHttpMessageHandler((request, ct) =>
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = JsonContent.Create(error)
            };
        });

        var httpClient = new HttpClient(handler);
        var resolver = new HandleResolver(httpClient, _serviceUri);

        var result = await resolver.ResolveAsync(handle, noCache: false);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_ServerError_ThrowsException()
    {
        // Arrange
        var handle = Handle.Create("test.atprot.app");

        var handler = new TestHttpMessageHandler((request, ct) =>
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        });

        var httpClient = new HttpClient(handler);
        var resolver = new HandleResolver(httpClient, _serviceUri);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => resolver.ResolveAsync(handle, noCache: false)
        );
    }

    [Fact]
    public async Task ResolveAsync_WithNoCache_SetsCacheControlHeader()
    {
        // Arrange
        var handle = Handle.Create("test.atprot.app");
        HttpRequestMessage? capturedRequest = null;

        var handler = new TestHttpMessageHandler((request, ct) =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new ResolveHandleResponse
                {
                    Did = "did:plc:test"
                })
            };
        });

        var httpClient = new HttpClient(handler);
        var resolver = new HandleResolver(httpClient, _serviceUri);

        // Act
        await resolver.ResolveAsync(handle, noCache: true);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.CacheControl?.NoCache);
    }

    [Fact]
    public async Task ResolveAsync_CancellationRequested_ThrowsTaskCanceledException()
    {
        // Arrange
        var handle = Handle.Create("test.atprot.app");
        var cts = new CancellationTokenSource();

        var handler = new TestHttpMessageHandler((request, ct) =>
        {
            throw new TaskCanceledException();
        });

        var httpClient = new HttpClient(handler);
        var resolver = new HandleResolver(httpClient, _serviceUri);

        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => resolver.ResolveAsync(handle, noCache: false, cts.Token)
        );
    }

    [Fact]
    public async Task ResolveAsync_MalformedResponse_ThrowsException()
    {
        // Arrange
        var handle = Handle.Create("test.atprot.app");

        var handler = new TestHttpMessageHandler((request, ct) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid json")
            };
        });

        var httpClient = new HttpClient(handler);
        var resolver = new HandleResolver(httpClient, _serviceUri);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(
            () => resolver.ResolveAsync(handle, noCache: false)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ResolveAsync_NullOrEmptyDID_ReturnsNull(string? did)
    {
        // Arrange
        var handle = Handle.Create("test.atprot.app");
        var response = new ResolveHandleResponse { Did = did };

        var handler = new TestHttpMessageHandler((request, ct) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(response)
            };
        });

        var httpClient = new HttpClient(handler);
        var resolver = new HandleResolver(httpClient, _serviceUri);

        // Act
        var result = await resolver.ResolveAsync(handle, noCache: false);

        // Assert
        Assert.Null(result);
    }
}
