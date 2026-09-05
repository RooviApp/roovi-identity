# PDS Integration Tests

This directory contains integration tests that run against a real PDS (Personal Data Server) instance using Testcontainers.

## How It Works

1. **Dockerfile.pds-test** - A test-friendly Dockerfile based on the official PDS image
2. **PdsTestContainer.cs** - A reusable test fixture that:
   - Builds a Docker image from the Dockerfile
   - Starts a PDS container with random ports and secrets
   - Provides helper methods to create accounts and invite codes
   - Automatically cleans up when tests complete

## Test Classes

Tests are organized by functionality:

### PdsContainerTests.cs
Basic PDS container functionality:
- Health endpoint verification
- Account creation
- Invite code generation
- Container configuration validation

### HandleResolverTests.cs
HandleResolver-specific tests:
- Handle-to-DID resolution
- Multiple handle resolution
- Custom PDS URL configuration
- Non-existent handle handling

### DIDResolverTests.cs
DIDResolver-specific tests:
- DID document resolution
- PDS service endpoint verification
- Multiple DID resolution

### IdentityResolverTests.cs
End-to-end identity resolution workflows:
- Complete handle-to-identity resolution
- DID-based resolution
- Error handling
- Multi-account scenarios

## Running Tests

Simply run the tests as you normally would:

```bash
dotnet test
```

**Prerequisites:**
- Docker Desktop must be running
- No special configuration needed - everything is automatic!

## Test Isolation

Each test class that uses `IClassFixture<PdsTestContainer>` gets its own isolated PDS instance:
- Fresh database
- Random admin password
- Random JWT secret
- Unique port mapping
- Automatic cleanup after all tests in the class complete

## Creating Test Accounts

```csharp
public class MyTests : IClassFixture<PdsTestContainer>
{
    private readonly PdsTestContainer _pds;

    public MyTests(PdsTestContainer pds)
    {
        _pds = pds;
    }

    [Fact]
    public async Task MyTest()
    {
        // Create an account
        var did = await _pds.CreateAccountAsync(
            handle: "testuser.test",
            email: "test@example.com",
            password: "password123"
        );

        // Use the PDS URL in your tests
        var pdsUrl = _pds.PdsUrl; // e.g., http://localhost:54321

        // Your test code here...
    }
}
```

## Performance

- **First run**: ~30-60 seconds (downloads PDS image and builds test image)
- **Subsequent runs**: ~5-10 seconds per test class (container startup time)
- Images are cached by Docker, so rebuilds are fast

## Debugging

If tests fail, you can get container logs:

```csharp
var logs = await _pds.GetLogsAsync();
Console.WriteLine(logs);
```

## Notes

- The PDS uses HTTP (not HTTPS) for testing simplicity
- When calling identity resolution methods, set `allowHttp: true`
- Accounts use `.test` domain handles (e.g., `user.test`)
- Each container gets random credentials for security
- Containers are automatically removed after tests complete
