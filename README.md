# Roovi Identity

The planned identity service for **Roovi**, an AT Protocol driven video app inspired by TikTok and YouTube Reels. This repository contains identity primitives and the service foundation; video feeds, uploads, and playback belong to the wider application.

**Status: early development.** The library implements DID and handle validation, handle resolution through XRPC, DID document retrieval, and PDS discovery. The API and OAuth flow are scaffolding, not a working authentication service. Passing tests do not establish full protocol conformance or production readiness.

## Repository layout

| Project | Responsibility |
| --- | --- |
| `ATProto.Primitives` | Identity types, JSON conversion, resolvers, PDS discovery, unfinished OAuth primitives |
| `ATProto.Primitives.Tests` | Unit tests, API smoke test, real PDS integration tests |
| `RooviApp.Identity.Api` | ASP.NET Core API scaffold with development Swagger |
| `RooviApp.Identity.AppHost` | Aspire 13.5 host for the API |
| `RooviApp.Identity.ServiceDefaults` | Health checks, service discovery, resilience, telemetry |

## Getting started

Install the **.NET 10 SDK** and Docker with Linux containers. `global.json` selects a stable .NET 10 SDK and Microsoft.Testing.Platform. `NuGet.Config` uses only nuget.org. The tests pull a pinned official PDS image; no public AT Protocol account or credentials are required.

```sh
git clone https://github.com/RooviApp/roovi-identity.git
cd roovi-identity
dotnet restore RooviApp.Identity.sln
dotnet build RooviApp.Identity.sln --configuration Release
dotnet test --solution RooviApp.Identity.sln --configuration Release --no-build
```

Run the API directly:

```sh
dotnet run --project RooviApp.Identity.Api --launch-profile http
```

Open [Swagger](http://localhost:5104/swagger). `POST /api/Authentication/login?username=...` remains a placeholder returning an empty 200 response; it does not authenticate or issue tokens. No identity-resolution HTTP endpoint is wired yet. The AppHost is available for Aspire-enabled environments.

## Tests and PR gate

GitHub Actions builds every project and runs **all tests** on pushes, pull requests, and merge-queue events. The required check is named **All tests**. There is no integration-test exclusion or continue-on-error setting. The command rejects skipped tests and runs with a five-minute test timeout; the job timeout is fifteen minutes. TRX results and Cobertura coverage are uploaded even on failure.

Run the same check locally:

```sh
dotnet test --solution RooviApp.Identity.sln --configuration Release --no-build --fail-skips on --minimum-expected-tests 185 --timeout 5m --report-trx --report-trx-filename all-tests.trx --results-directory TestResults --coverlet --coverlet-output-format cobertura
```

The 185-test minimum catches accidental loss of test discovery. Update it deliberately when reorganizing/removing tests; adding tests does not require changing it.

For a quick unit-only development run without Docker:

```sh
dotnet test --project ATProto.Primitives.Tests/ATProto.Tests.csproj --filter-not-namespace "ATProto.Tests.Integration*"
```

Integration tests create temporary accounts on a real PDS and resolve them through a local PLC test double. The double validates genesis signatures and DID hashes with the reference PLC library, but does not implement the full PLC operation history/recovery protocol. It never writes to the public PLC directory. See [integration notes](ATProto.Primitives.Tests/Integration/README.md).

Direct NuGet dependencies and GitHub Actions use current stable releases. Transitive dependencies follow the versions selected by their upstream packages. Dependabot checks NuGet and Actions weekly. To audit:

```sh
dotnet list RooviApp.Identity.sln package --outdated
dotnet list RooviApp.Identity.sln package --vulnerable --include-transitive
```

## Next milestones

- Wire resolver dependencies and expose a defined identity-resolution API; `AddATProto` currently leaves HTTP client and handle resolver setup to callers.
- Complete OAuth discovery, authorization, callback validation, and session handling; replace the login placeholder.
- Harden resolution: exact handle back-reference checks, matching requested/returned DIDs, absolute PDS service IDs, URL handling, request limits, and SSRF protections.
- Define the boundary between Roovi sessions and AT Protocol credentials before integrating the video app.

## License and protocol references

[MIT](LICENSE), copyright Cory Ball. See the [AT Protocol identity guide](https://atproto.com/guides/identity) and [specifications](https://atproto.com/specs/atp).
