# PDS integration fixture

See [integration notes](../README.md) for the current architecture and commands.

The fixture runs a pinned official PDS image and a local PLC creation/resolution test double. It uses XRPC for account/invite creation and an ephemeral data mount. It does not build a custom Docker image or use pdsadmin. All PDS tests run in CI as part of the required All tests check.
