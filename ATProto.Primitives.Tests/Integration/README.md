# Integration tests

External tests are excluded from the default unit-test CI job.

- `Identity/IdentityResolutionTests.cs` resolves a hard-coded public handle through bsky.social and plc.directory. Network availability and account changes affect results.
- `Pds/` builds a Linux PDS image with Docker/Testcontainers. The fixture currently fails container startup, assumes a `pdsadmin` executable inside the image, and points at the public PLC directory. Account creation may write to public infrastructure if the fixture succeeds. This is experimental infrastructure, not an isolated local environment.

From the repository root:

```sh
dotnet test ATProto.Primitives.Tests/ATProto.Tests.csproj --filter "FullyQualifiedName~Integration.Identity"
dotnet test ATProto.Primitives.Tests/ATProto.Tests.csproj --filter "FullyQualifiedName~Integration.Pds"
```

Use `--filter "FullyQualifiedName!~Integration"` for deterministic unit tests. Older PDS documents describe intended behavior and examples; they do not establish that the fixture works with the current image. Repair the fixture and isolate PLC operations before making these tests a required CI check.
