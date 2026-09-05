using System.Text.Json;
using System.Text.Json.Serialization;

namespace ATProto.Primitives.JsonConverters;

public class DIDDocumentJsonCoverter : JsonConverter<DIDDocument>
{
    public override DIDDocument? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        var response = JsonSerializer.Deserialize<DIDDocumentJsonResponse>(ref reader, options);

        if (response is null)
        {
            throw new JsonException("Failed to deserialize DID Document");
        }

        var document = new DIDDocument
        {
            Id = DID.Create(response.Id),
            AlsoKnownAs = response.AlsoKnownAs
                .Select(RFC3968Uri.Parse)
                .ToList(),
            Service = response.Service
                .Select(s => new DIDService
                {
                    Id = DIDRelativeUri.Parse(s.Id),
                    Type = s.Type,
                    ServiceEndpoint = DIDServiceEndpoint.FromUri(s.ServiceEndpoint),
                })
                .ToList(),
            VerificationMethod = response.VerificationMethod
                .Select(vm => new DIDVerificationInstance
                {
                    Id = DIDRelativeUri.Parse(vm.Id),
                    Type = vm.Type,
                    Controller = DID.Create(vm.Controller),
                    PublicKeyMultibase = vm.PublicKeyMultibase,
                })
                .ToList(),
        };

        return document;
    }

    public override void Write(
        Utf8JsonWriter writer,
        DIDDocument value,
        JsonSerializerOptions options)
    {
        var response = new DIDDocumentJsonResponse
        {
            Id = value.Id.Value,
            AlsoKnownAs = value.AlsoKnownAs
                .Select(ak => ak.Value)
                .ToList(),
            Service = value.Service
                .Select(s => new DIDDocumentServiceJsonResponse
                {
                    Id = s.Id.Value,
                    Type = s.Type,
                    ServiceEndpoint = s.ServiceEndpoint.Uri!,
                })
                .ToList(),
            VerificationMethod = value.VerificationMethod
                .Select(vm => new DIDDocumentVerificationMethodJsonResponse
                {
                    Id = vm.Id.Value,
                    Type = vm.Type,
                    Controller = vm.Controller.Value,
                    PublicKeyMultibase = vm.PublicKeyMultibase ?? string.Empty,
                })
                .ToList(),
        };

        JsonSerializer.Serialize(writer, response, options);
    }
}

public class DIDDocumentJsonResponse
{
    [JsonPropertyName("@context")]
    public List<string> Context { get; set; } = [];

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("alsoKnownAs")]
    public List<string> AlsoKnownAs { get; set; } = [];

    [JsonPropertyName("verificationMethod")]
    public List<DIDDocumentVerificationMethodJsonResponse> VerificationMethod { get; set; } = [];

    [JsonPropertyName("service")]
    public List<DIDDocumentServiceJsonResponse> Service { get; set; } = [];
}

public class DIDDocumentVerificationMethodJsonResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("controller")]
    public required string Controller { get; set; }

    [JsonPropertyName("publicKeyMultibase")]
    public string PublicKeyMultibase { get; set; } = string.Empty;
}

public class DIDDocumentServiceJsonResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("serviceEndpoint")]
    public required string ServiceEndpoint { get; set; }
}
