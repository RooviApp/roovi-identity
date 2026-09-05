using ATProto.Primitives;

namespace ATProto.Identity;

public interface IDIDResolver
{
    Task<DIDDocument> ResolveAsync(DID did, bool noCache, string plcDirectoryUrl, bool allowHttp, CancellationToken ct);
}
