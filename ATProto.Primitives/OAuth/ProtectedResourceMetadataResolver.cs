
using System.Data;
using System.Net.Http.Headers;

namespace ATProto.OAuth;

public sealed class ProtectedResourceMetadataResolver(HttpClient httpClient)
{
    public async Task GetResourceServerMetadata(string pdsUrl, bool noCache, CancellationToken ct = default)
    {
        var uri = new Uri(pdsUrl);
        var scheme = uri.Scheme;
        var origin = uri.Host;

        if (scheme != "https" && scheme != "http")
        {
            throw new ArgumentException($"Invalid protected resource metadata URL protocol: {pdsUrl}");
        }


    }

    private async Task FetchMetadata(string origin, bool noCache, CancellationToken ct = default)
    {
        var uri = new Uri($"{origin}/.well-known/oauth-protected-resource");

        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.CacheControl = new CacheControlHeaderValue
        {
            NoCache = noCache,
        };

        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Unexpected content type for {uri}");
        }


    }

}
