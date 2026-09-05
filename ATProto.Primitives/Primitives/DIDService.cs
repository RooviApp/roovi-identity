
namespace ATProto.Primitives;

public sealed class DIDService
{
    public required DIDRelativeUri Id { get; set; }
    public required string Type { get; set; }
    public required DIDServiceEndpoint ServiceEndpoint { get; set; }
}
