# Integration tests

Every integration test runs in the required **All tests** PR check. Docker with Linux containers is required; public AT Protocol services and user credentials are not.

- `Api/ApiSmokeTests.cs` starts the actual API with WebApplicationFactory and checks its generated OpenAPI document.
- `Pds/` uses one xUnit collection fixture shared across the PDS tests. A real official PDS image is pinned by digest, with ephemeral data and random secrets. Accounts and invites are created through XRPC, not a host-side administration script.
- `Identity/IdentityResolutionTests.cs` creates a fresh local account and checks complete identity resolution instead of relying on a hard-coded public handle.
- `Pds/plc.cjs` is a local creation/resolution test double. It uses the reference PLC library bundled in the pinned PDS image to validate genesis signatures and DID hashes and format documents. It is not a full PLC server and does not test update history, recovery, or tombstones.

Both containers share a temporary Docker network. PDS points only to the local PLC container. No public PLC operations, AppView, or crawler are configured. The fixture has bounded startup, captures container logs on startup failure, and disposes containers/network after the run.

`PdsUrl` is Docker's randomly mapped host address for requests. `ServiceUrl` is the PDS's advertised identity endpoint inside the test environment. Tests distinguish these addresses deliberately.

```sh
dotnet test --solution RooviApp.Identity.sln --configuration Release --fail-skips on --report-trx
```

This includes unit and integration tests. See the root README for the exact CI command, coverage options, and unit-only development filter.
