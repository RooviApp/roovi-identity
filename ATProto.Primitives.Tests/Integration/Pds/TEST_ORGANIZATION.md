# Test Organization Summary

## Overview

The PDS integration tests have been organized into focused test classes based on functionality. This improves maintainability, readability, and makes it easier to run specific test suites.

## New Test Class Organization

### 1. **PdsContainerTests.cs** (6 tests)
**Purpose**: Test basic PDS container functionality and infrastructure

**Tests**:
- `PdsHealthEndpoint_ShouldReturnVersion` - Verifies health endpoint
- `CreateAccount_ShouldSucceed` - Tests account creation
- `CreateInviteCode_ShouldReturnValidCode` - Tests invite code generation
- `MultipleAccounts_CanBeCreatedOnSamePds` - Tests multiple account creation
- `PdsUrl_ShouldBeAccessible` - Validates container URL
- `AdminPassword_ShouldBeAvailable` - Validates admin credentials

**When to use**: Testing container setup, account management, administrative functions

---

### 2. **HandleResolverTests.cs** (4 tests)
**Purpose**: Test HandleResolver functionality with a real PDS

**Tests**:
- `ResolveAsync_WithValidHandle_ResolvesToDID` - Basic handle-to-DID resolution
- `ResolveAsync_WithMultipleHandles_ResolvesIndependently` - Multiple handle resolution
- `ResolveAsync_WithCustomPdsUrl_ResolvesThroughSpecificInstance` - Custom PDS configuration
- `ResolveAsync_WithNonExistentHandle_ReturnsNull` - Error handling

**When to use**: Testing handle resolution logic, custom PDS configurations

---

### 3. **DIDResolverTests.cs** (3 tests)
**Purpose**: Test DIDResolver functionality with PLC directory

**Tests**:
- `ResolveAsync_WithValidDID_ResolvesDIDDocument` - Basic DID document resolution
- `ResolveAsync_DIDDocument_ContainsAtprotoPersonalDataServerService` - Service endpoint validation
- `ResolveAsync_WithMultipleDIDs_ResolvesCorrectDocuments` - Multiple DID resolution

**When to use**: Testing DID document resolution, PLC directory integration

---

### 4. **IdentityResolverTests.cs** (5 tests)
**Purpose**: Test end-to-end identity resolution workflows

**Tests**:
- `GetDIDDocument_WithHandle_ResolvesCompleteIdentity` - Complete handle-based resolution
- `GetDIDDocument_WithDID_ResolvesDirectly` - DID-based resolution
- `GetDIDDocument_WithInvalidHandle_ThrowsException` - Error handling
- `GetDIDDocument_EndToEnd_CompleteWorkflow` - Full workflow demonstration
- `GetDIDDocument_WithMultipleAccounts_ResolvesCorrectly` - Multiple account scenarios

**When to use**: Testing complete identity resolution flows, integration scenarios

---

## Total Test Count

| Test Class | Test Count | Focus Area |
|------------|-----------|------------|
| PdsContainerTests | 6 | Container & Admin |
| HandleResolverTests | 4 | Handle Resolution |
| DIDResolverTests | 3 | DID Resolution |
| IdentityResolverTests | 5 | End-to-End |
| **Total** | **18** | **All Areas** |

## Running Tests

### Run All PDS Tests
```powershell
dotnet test --filter "FullyQualifiedName~Pds"
```

### Run by Test Class
```powershell
# Container functionality tests
dotnet test --filter "PdsContainerTests"

# Handle resolution tests
dotnet test --filter "HandleResolverTests"

# DID resolution tests
dotnet test --filter "DIDResolverTests"

# End-to-end tests
dotnet test --filter "IdentityResolverTests"
```

### Run Specific Test
```powershell
dotnet test --filter "PdsHealthEndpoint_ShouldReturnVersion"
```

## Benefits of This Organization

✅ **Focused Tests**: Each class has a clear, single responsibility
✅ **Better Naming**: Test names clearly indicate what's being tested
✅ **Easier Navigation**: Find tests related to specific components quickly
✅ **Selective Running**: Run only the tests you need
✅ **Parallel Execution**: Each class can run in parallel (with separate PDS instances)
✅ **Maintainability**: Changes to one component only affect related tests
✅ **Documentation**: Test class names serve as documentation

## Test Isolation

Each test class uses `IClassFixture<PdsTestContainer>`, which means:
- Each test class gets its own PDS container instance
- Complete isolation between test classes
- Can run in parallel without conflicts
- Fresh database for each test class

## Previous Organization

**Before**:
- `PdsIntegrationTests.cs` - 5 basic tests
- `AdvancedIdentityResolverTests.cs` - 9 advanced tests
- Mixed concerns in both files

**After**:
- `PdsContainerTests.cs` - Container/admin tests
- `HandleResolverTests.cs` - Handle resolution tests
- `DIDResolverTests.cs` - DID resolution tests
- `IdentityResolverTests.cs` - End-to-end tests
- Clear separation of concerns

## Deprecated Files

The following files have been replaced and can be removed:
- ~~`PdsIntegrationTests.cs`~~ → Split into new test classes
- ~~`AdvancedIdentityResolverTests.cs`~~ → Split into new test classes

## Migration Notes

If you have references to old test files:
- `PdsIntegrationTests` → Use `PdsContainerTests` for container tests
- `AdvancedIdentityResolverTests` → Use `HandleResolverTests`, `DIDResolverTests`, or `IdentityResolverTests` based on what you're testing

## Examples

### Testing Container Setup
```csharp
// Use PdsContainerTests
public class PdsContainerTests(PdsTestContainer pds) : IClassFixture<PdsTestContainer>
{
    [Fact]
    public async Task MyContainerTest()
    {
        var did = await pds.CreateAccountAsync(...);
        // Test container functionality
    }
}
```

### Testing Handle Resolution
```csharp
// Use HandleResolverTests
public class HandleResolverTests(PdsTestContainer pds) : IClassFixture<PdsTestContainer>
{
    [Fact]
    public async Task MyHandleTest()
    {
        var resolver = new HandleResolver(_httpClient, new Uri(pds.PdsUrl));
        // Test handle resolution
    }
}
```

### Testing Complete Workflows
```csharp
// Use IdentityResolverTests
public class IdentityResolverTests(PdsTestContainer pds) : IClassFixture<PdsTestContainer>
{
    [Fact]
    public async Task MyWorkflowTest()
    {
        var identityResolver = new IdentityResolver(...);
        // Test end-to-end workflows
    }
}
```

## Next Steps

1. ✅ Review the new test organization
2. ✅ Run tests to verify everything works: `dotnet test --filter "FullyQualifiedName~Pds"`
3. ⬜ Delete old test files if you're satisfied with the new organization
4. ⬜ Update any documentation or scripts that reference old test files
5. ⬜ Add new tests using the appropriate test class

## Questions?

- **Which test class should I use?**
  - Testing container/admin features? → `PdsContainerTests`
  - Testing handle resolution? → `HandleResolverTests`
  - Testing DID resolution? → `DIDResolverTests`
  - Testing complete workflows? → `IdentityResolverTests`

- **Can tests share a PDS container?**
  - Tests within the same class share a container
  - Different test classes get different containers
  - Use `[Collection]` attribute to share across classes if needed
