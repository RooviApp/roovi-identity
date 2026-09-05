using ATProto.Primitives;

namespace ATProto.Identity;

public sealed class ResolvedIdentity
{
    public required DID DID { get; set; }
    public required Uri Pds { get; set; }
}
