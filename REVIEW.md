# Repository review

## .NET 10 upgrade validation

- All five projects target net10.0; direct dependencies are on current stable releases.
- Aspire 13.5.3, OpenTelemetry 1.18.0, Swashbuckle 10.2.3, Testcontainers 4.14.0, and xUnit v3 4.0.0 are installed.
- Tests use .NET 10 Microsoft.Testing.Platform with TRX reporting and Coverlet coverage.
- Release build passed with zero warnings/errors. All 185 tests passed, zero skipped (152 unit tests and 33 integration tests).
- NuGet audit reports no known vulnerable direct or transitive packages across the solution. Upstream packages retain their own compatible transitive versions.
- The previous 31 failing PDS tests are repaired. A local PLC creation/resolution test double validates genesis operations; no public account or PLC writes are needed.
- The All tests workflow runs the whole suite without filters and rejects skips. The required check is intended for main branch protection.

## Remaining implementation gaps

- AuthenticationController returns success without authenticating. OAuth implementations are unfinished.
- The API does not reference/register the primitives library. AddATProto does not register the handle resolver or HTTP client dependencies.
- IdentityResolver uses a substring handle back-reference check and allows a missing alias list. Its absolute PDS service ID comparison uses `$atproto_pds` instead of `#atproto_pds`.
- DIDResolver needs requested/document DID matching and URL construction review; noCache is unused there.
- Outbound resolution needs SSRF defenses, bounded requests, and negative-path tests before exposure to untrusted input.
- PLC update history/recovery is outside the local creation/resolution test double's scope.

The repository remains an early development project, not a production authentication service.
