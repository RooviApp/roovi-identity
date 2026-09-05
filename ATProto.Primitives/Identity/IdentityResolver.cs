using ATProto.Primitives;

namespace ATProto.Identity;

public class IdentityResolver (IHandleResolver handleResolver, IDIDResolver dIDResolver)
{
    public async Task<ResolvedIdentity> GetDIDDocument(string input, bool noCache, string plcDirectoryUrl = "https://plc.directory/", bool allowHttp = false, CancellationToken ct = default)
    {
        DIDDocument? document = null;
        if (Handle.IsResolvedHandle(input, out DID? resolvedHandle))
        {
            document = await GetDocumentFromDidAsync(resolvedHandle!, noCache, plcDirectoryUrl, allowHttp, ct);
        }
        else
        {
            document=  await GetDocumentFromHandleAsync(input, noCache, plcDirectoryUrl, allowHttp, ct);
        }
        var service = document.Service.FirstOrDefault(s => s.Type == "AtprotoPersonalDataServer" && (s.Id.Value.StartsWith('#') ? s.Id.Value == "#atproto_pds" : s.Id.Value == $"{document.Id.Value}$atproto_pds"));

        if (service is null)
        {
            throw new Exception($"No valid 'AtprotoPersonalDataServer' service found in {document.Id.Value} DID document");
        }

        var serviceEndpoint = service.ServiceEndpoint.Type switch
        {
            DIDServiceEndpoint.EndpointType.Uri => new Uri(service.ServiceEndpoint.Uri!),
            _ => throw new Exception($"Unsupported service endpoint type: {service.ServiceEndpoint.Type}"),
        };

        return new ResolvedIdentity
        {
            DID = document.Id,
            Pds = serviceEndpoint,
        };
    }
    private async Task<DIDDocument> GetDocumentFromDidAsync(DID did, bool noCache, string plcDirectoryUrl, bool allowHttp, CancellationToken ct = default)
    {
        return await dIDResolver.ResolveAsync(did, noCache, plcDirectoryUrl, allowHttp, ct);
    }

    private async Task<DIDDocument> GetDocumentFromHandleAsync(string handle, bool noCache, string plcDirectoryUrl, bool allowHttp, CancellationToken ct = default)
    {
        var normalizedHandle = Handle.Create(handle);
        var did = await handleResolver.ResolveAsync(normalizedHandle, noCache, ct);

        if (did is null)
        {
            throw new Exception($"Handle {normalizedHandle.Value} does not resolve to a DID");
        }

        ct.ThrowIfCancellationRequested();

        var document = await dIDResolver.ResolveAsync(did, noCache, plcDirectoryUrl, allowHttp, ct);

        if (document is null)
        {
            throw new Exception($"No DID document found for {did.Value}");
        }

        if (!document.AlsoKnownAs?.Any(x => x.Value.Contains($"at://{normalizedHandle}")) ?? false)
        {
            throw new Exception($"Did document for {did.Value} does not include the handle {normalizedHandle.Value}");
        }

        return document;
    }
}
