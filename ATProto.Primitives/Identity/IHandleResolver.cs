using ATProto.Primitives;

namespace ATProto.Identity;

public interface IHandleResolver
{
    Task<DID?> ResolveAsync(Handle handle, bool noCache, CancellationToken ct = default);
}
