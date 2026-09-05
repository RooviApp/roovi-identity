namespace ATProto.Primitives;

public sealed class DIDDocument
{
    public required DID Id { get; set; }
    public List<RFC3968Uri> AlsoKnownAs { get; set; } = [];
    public List<DIDService> Service { get; set; } = [];

    public List<DIDVerificationMethod> Authentication { get; set; } = [];

    public List<DIDVerificationInstance> VerificationMethod { get; set; } = [];
}
