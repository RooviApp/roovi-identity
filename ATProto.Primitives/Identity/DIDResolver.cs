using ATProto.Primitives;
using ATProto.Primitives.JsonConverters;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ATProto.Identity;

public class DIDResolver(HttpClient httpClient) : IDIDResolver
{
    public async Task<DIDDocument> ResolveAsync(DID did, bool noCache, string plcDirectoryUrl, bool allowHttp, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var method = did.ExtractMethod();

        return method switch
        {
            "web" => await ResolveWebMethodAsync(did, allowHttp, ct),
            "plc" => await ResolvePlcMethodAsync(did, plcDirectoryUrl, ct),
            _ => throw new Exception($"Unsupported DID method: {method}"),
        };
    }

    private async Task<DIDDocument> ResolvePlcMethodAsync(DID did, string plcDirectoryUrl, CancellationToken ct = default)
    {
        // Although we've verified the did starts with "did:plc", we should still check if the msid is valid
        did.AssertValidPlc();

        var url = new Uri($"{plcDirectoryUrl}{WebUtility.UrlEncode(did.Value)}");
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/did+ld+json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var document = await response.Content.ReadFromJsonAsync<DIDDocument>(new JsonSerializerOptions
        {
            Converters = { new DIDDocumentJsonCoverter() }
        }, ct);

        if (document is null)
        {
            throw new Exception("Could not retrieve Did Document");
        }

        return document;
    }

    private async Task<DIDDocument> ResolveWebMethodAsync(DID did, bool allowHttp = false, CancellationToken ct = default)
    {
        did.AssertMsid(DID.DID_WEB_PREFIX_LENGTH);

        var hostIdx = DID.DID_WEB_PREFIX_LENGTH;
        var pathIdx = did.Value.IndexOf(':', hostIdx);

        var host = pathIdx == -1 ? did.Value[hostIdx..] : did.Value[hostIdx..pathIdx];
        var path = pathIdx == -1 ? "" : did.Value[(pathIdx + 1)..];

        Uri? url = null;
        try
        {
            var protocol = host.Contains("localhost") ? "http" : "https";
            url = new Uri($"{protocol}://{host.Replace("%3A", ":")}{path.Replace(':', '/')}");
        }
        catch (Exception ex)
        {
            throw new Exception("Invalid Web DID", ex);
        }

        if (url is null)
        {
            throw new Exception("Invalid Web DID");
        }

        if (url.AbsolutePath == "/")
        {
            url = new Uri(url, "/.well-known/did.json");
        }
        else
        {
            url = new Uri(url, $"{url.AbsolutePath}/did.json");
        }

        if (!allowHttp && url.Scheme != "https")
        {
            throw new Exception("Resolution of 'http' did:web is not allowed");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/did+ld+json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var document = await response.Content.ReadFromJsonAsync<DIDDocument>(new JsonSerializerOptions
        {
            Converters = { new DIDDocumentJsonCoverter() }
        }, ct);

        if (document is null)
        {
            throw new Exception("Failed to parse DID Document");
        }

        return document;
    }
}
