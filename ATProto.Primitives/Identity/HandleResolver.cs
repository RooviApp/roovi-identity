using ATProto.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace ATProto.Identity;

public sealed partial class HandleResolver(
    [NotNull] HttpClient httpClient,
    [NotNull] Uri ServiceUri) : IHandleResolver
{

    public async Task<DID?> ResolveAsync(Handle handle, bool noCache, CancellationToken ct = default)
    {
        var url = new Uri(ServiceUri, "/xrpc/com.atproto.identity.resolveHandle");
        var urlBuilder = new UriBuilder(url);
        var query = HttpUtility.ParseQueryString(urlBuilder.Query);
        query["handle"] = handle.Value;
        urlBuilder.Query = query.ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, urlBuilder.Uri);
        request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = noCache };

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errorData = JsonSerializer.Deserialize<XrpcError>(
                content,
                options: new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (errorData?.Error == "InvalidRequest" &&
                errorData?.Message == "Unable to resolve handle")
            {
                return null;
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to resolve handle");
        }

        var payload = JsonSerializer.Deserialize<ResolveHandleResponse>(
            content,
            options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        return string.IsNullOrEmpty(payload?.Did) ? null : DID.Create(payload.Did);
    }
}

public sealed class ResolveHandleResponse
{
    public string? Did { get; set; }
}

public sealed class XrpcError
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
