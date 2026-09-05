# Initial publication review

Reviewed September 4, 2026. Suitable for publication as an early development project, not deployment as an authentication service.

## Validation

- Release solution build: passed, zero warnings and errors on the final run.
- Unit suite: 152 passed, zero failed or skipped.
- Live identity smoke test: one passed.
- PDS integration suite: 31 failed during fixture startup. Docker reported that the container was not running. These failures remain unresolved and are documented, not silently skipped.
- GitHub Actions is configured to build and run unit tests. It has not run on GitHub yet.
- No obvious credential patterns were found in the publishable source/configuration scan. Build outputs, IDE state, logs, and TRX files are ignored.

## Dependency audit

The OTLP exporter was updated from 1.9.0 to 1.15.3 to address [GHSA-4625-4j76-fww9](https://github.com/advisories/GHSA-4625-4j76-fww9). The final NuGet audit reported no vulnerable packages in the API, primitives, or service-defaults projects.

Remaining transitive advisories in development/test infrastructure need follow-up:

| Project | Dependency | Advisory |
| --- | --- | --- |
| AppHost | KubernetesClient 15.0.1 | [GHSA-w7r3-mgwf-4mqq](https://github.com/advisories/GHSA-w7r3-mgwf-4mqq) |
| Tests | SSH.NET 2023.0.0 | [GHSA-q939-rpr3-3284](https://github.com/advisories/GHSA-q939-rpr3-3284) |
| Tests | System.Text.Json 8.0.0 | [GHSA-hh2w-p6rv-4g7w](https://github.com/advisories/GHSA-hh2w-p6rv-4g7w), [GHSA-8g4q-xg66-9fp4](https://github.com/advisories/GHSA-8g4q-xg66-9fp4) |

Upgrade and validate Aspire/Testcontainers dependencies before relying on this infrastructure beyond development.

## Implementation gaps

- AuthenticationController returns success without authenticating. OAuth implementations are unfinished.
- The API does not reference or register the primitives library. AddATProto does not register the handle resolver or HTTP client dependencies.
- IdentityResolver uses a substring handle back-reference check and allows a missing alias list. Its absolute PDS service ID comparison uses `$atproto_pds` instead of `#atproto_pds`.
- DIDResolver needs requested/document DID matching and URL construction review; noCache is unused there.
- Outbound identity resolution needs SSRF defenses, bounded requests, and deterministic negative-path tests before exposure to untrusted input.
- The PDS fixture needs startup diagnostics, a supported account-creation mechanism, and a local PLC service to avoid public directory writes.

These findings define follow-up implementation work; this preparation pass does not claim to resolve them.
