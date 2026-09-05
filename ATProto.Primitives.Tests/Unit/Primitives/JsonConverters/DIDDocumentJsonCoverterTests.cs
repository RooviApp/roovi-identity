using ATProto.Primitives.JsonConverters;
using ATProto.Primitives;
using System.Text.Json;

namespace ATProto.Tests.Primitives.JsonConverters;

public class DIDDocumentJsonCoverterTests
{
    private readonly JsonSerializerOptions _options;

    public DIDDocumentJsonCoverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new DIDDocumentJsonCoverter() }
        };
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsDIDDocument()
    {
        // Arrange
        var json = """
        {
            "@context": ["https://www.w3.org/ns/did/v1"],
            "id": "did:plc:7iza6de2dwap2sbkpav7c6c6abcdef",
            "alsoKnownAs": ["at://testuser.roovi.app"],
            "verificationMethod": [{
                "id": "did:plc:7iza6de2dwap2sbkpav7c6c6abcdef#atproto",
                "type": "MultiKey",
                "controller": "did:plc:7iza6de2dwap2sbkpav7c6c6abcdef",
                "publicKeyMultibase": "z123456789"
            }],
            "service": [{
                "id": "#atproto_pds",
                "type": "AtprotoPersonalDataServer",
                "serviceEndpoint": "https://example.roovi.network"
            }]
        }
        """;

        // Act
        var document = JsonSerializer.Deserialize<DIDDocument>(json, _options);

        // Assert
        Assert.NotNull(document);
        Assert.Equal("did:plc:7iza6de2dwap2sbkpav7c6c6abcdef", document.Id.Value);
        Assert.Single(document.AlsoKnownAs);
        Assert.Equal("at://testuser.roovi.app", document.AlsoKnownAs[0].Value);
        Assert.Single(document.VerificationMethod);
        Assert.Equal("did:plc:7iza6de2dwap2sbkpav7c6c6abcdef#atproto", document.VerificationMethod[0].Id.Value);
        Assert.Single(document.Service);
        Assert.Equal("#atproto_pds", document.Service[0].Id.Value);
    }

    [Fact]
    public void Serialize_ValidDocument_ReturnsCorrectJson()
    {
        // Arrange
        var document = new DIDDocument
        {
            Id = DID.Create("did:plc:7iza6de2dwap2sbkpav7c6c6abcdef"),
            AlsoKnownAs =
            [
                RFC3968Uri.Parse("https://example.com/alice")
            ],
            VerificationMethod =
            [
                new DIDVerificationInstance
                {
                    Id = DIDRelativeUri.Parse("#key-1"),
                    Type = "MultiKey",
                    Controller = DID.Create("did:plc:7iza6de2dwap2sbkpav7c6c6abcdef"),
                    PublicKeyMultibase = "z123456789"
                }
            ],
            Service =
            [
                new DIDService
                {
                    Id = DIDRelativeUri.Parse("#atproto"),
                    Type = "AtprotoPersonalDataServer",
                    ServiceEndpoint = DIDServiceEndpoint.FromUri("https://example.com/xrpc")
                }
            ]
        };

        // Act
        var json = JsonSerializer.Serialize(document, _options);
        var deserializedDocument = JsonSerializer.Deserialize<DIDDocument>(json, _options);

        // Assert
        Assert.NotNull(deserializedDocument);
        Assert.Equal(document.Id.Value, deserializedDocument.Id.Value);
        Assert.Equal(
            document.AlsoKnownAs[0].Value,
            deserializedDocument.AlsoKnownAs[0].Value
        );
        Assert.Equal(
            document.VerificationMethod[0].Id,
            deserializedDocument.VerificationMethod[0].Id
        );
        Assert.Equal(
            document.Service[0].Id,
            deserializedDocument.Service[0].Id
        );
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DIDDocument>(invalidJson, _options)
        );
    }

    [Fact]
    public void Deserialize_MissingRequiredFields_ThrowsJsonException()
    {
        // Arrange
        var jsonMissingId = @"{
            '@context': ['https://www.w3.org/ns/did/v1'],
            'verificationMethod': [],
            'service': []
        }";

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DIDDocument>(jsonMissingId, _options)
        );
    }
}
