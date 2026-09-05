using System.Net.Http.Json;
using System.Net;

namespace ATProto.Tests.TestUtilities;

public class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    public TestHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>? handler = null)
    {
        _handler = handler != null
            ? (req, ct) => Task.FromResult(handler(req, ct))
            : (req, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { })
            });
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return _handler(request, cancellationToken);
    }
}
