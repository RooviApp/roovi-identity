namespace ATProto.Primitives;

public sealed class DIDVerificationMethod
{
    public DIDVerificationMethod() { }

    public DIDRelativeUri? VerificationReference { get; set; }
    public DIDVerificationInstance? VerificationInstance { get; set; }

    public DIDVerificationMethod(DIDVerificationInstance instance)
    {
        VerificationInstance = instance;
    }

    public DIDVerificationMethod(DIDRelativeUri reference)
    {
        VerificationReference = reference;
    }
}

public sealed class  DIDVerificationInstance
{
    public DIDVerificationInstance() { }

    public required DIDRelativeUri Id { get; set; }
    public required string Type { get; set; }
    public required DID Controller { get; set; }

    public Dictionary<string, object> PublicKeyJwk { get; set; } = [];

    public string? PublicKeyMultibase { get; set; }
}
