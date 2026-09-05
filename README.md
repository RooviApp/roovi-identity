# Roovi Identity

The planned identity service for **Roovi**, an AT Protocol driven video app inspired by TikTok and YouTube Reels. This repository contains identity primitives and the service foundation; video feeds, uploads, and playback belong to the wider application.

**Status: early development.** The library implements DID and handle validation, handle resolution through XRPC, DID document retrieval, and PDS discovery. The API and OAuth flow are scaffolding, not a working authentication service. Passing unit tests do not establish full protocol conformance or production readiness.

## Repository layout

| Project | Current responsibility |
| --- | --- |
| `ATProto.Primitives` | Identity types, JSON conversion, resolvers, PDS discovery, unfinished OAuth primitives |
| `ATProto.Primitives.Tests` | xUnit unit tests and external integration tests |
| `RooviApp.Identity.Api` | ASP.NET Core API scaffold with development Swagger |
| `RooviApp.Identity.AppHost` | Aspire 9.0 host for the API |
| `RooviApp.Identity.ServiceDefaults` | Health checks, service discovery, resilience, telemetry |

## Getting started

Prerequisites: .NET 10 SDK and .NET 8 / ASP.NET Core 8 runtimes (projects target `net8.0`). CI installs both SDKs. Docker with Linux containers is additionally required for PDS integration tests. `NuGet.Config` uses only nuget.org; private package feeds are not required.

```sh
git clone https://github.com/RooviApp/roovi-identity.git
cd roovi-identity
dotnet restore RooviApp.Identity.sln
dotnet build RooviApp.Identity.sln --configuration Release
dotnet test ATProto.Primitives.Tests/ATProto.Tests.csproj --configuration Release --no-build --filter "FullyQualifiedName!~Integration"
```

Run the API directly:

```sh
dotnet run --project RooviApp.Identity.Api --launch-profile http
```

Open [Swagger](http://localhost:5104/swagger). The existing `POST /api/Authentication/login?username=...` endpoint is a placeholder returning an empty 200 response; it does not authenticate anyone or issue tokens. No identity-resolution HTTP endpoint is wired yet. The AppHost is available for Aspire-enabled development environments.

## Tests

GitHub Actions builds the whole solution and runs deterministic unit tests on pushes and pull requests, uploading TRX results. Unit namespaces are not uniform, so the filter excludes `Integration` rather than assuming a `Unit` namespace.

Live identity smoke test (requires public network access and a hard-coded public account):

```sh
dotnet test ATProto.Primitives.Tests/ATProto.Tests.csproj --configuration Release --filter "FullyQualifiedName~Integration.Identity"
```

Experimental PDS tests (Docker and network required):

```sh
dotnet test ATProto.Primitives.Tests/ATProto.Tests.csproj --configuration Release --filter "FullyQualifiedName~Integration.Pds"
```

An unfiltered `dotnet test` includes external tests. PDS tests are excluded from the default CI gate: the fixture currently fails container startup, assumes `pdsadmin` exists inside the image, and uses the public PLC directory for account creation/resolution. It needs repair and isolation before it can serve as a reliable CI gate. See [integration notes](ATProto.Primitives.Tests/Integration/README.md).

## Next milestones

- Wire resolver dependencies and expose a defined identity-resolution API; `AddATProto` currently leaves HTTP client and handle resolver setup to callers.
- Complete OAuth discovery, authorization, callback validation, and session handling; replace the login placeholder.
- Harden resolution: exact handle back-reference checks, matching requested/returned DIDs, absolute PDS service IDs, URL handling, request limits, and SSRF protections.
- Add deterministic resolver/API tests, then repair and isolate the PDS fixture from the public PLC directory.
- Define the boundary between Roovi sessions and AT Protocol credentials before integrating the video app.

## License and protocol references

[MIT](LICENSE), copyright Cory Ball. See the [AT Protocol identity guide](https://atproto.com/guides/identity) and [specifications](https://atproto.com/specs/atp).
