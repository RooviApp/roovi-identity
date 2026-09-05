using System.Security.Cryptography;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace ATProto.Tests.Integration.Pds;

/// <summary>
/// Testcontainers fixture for running a real PDS instance during integration tests.
/// Each test class can get its own isolated PDS instance.
/// </summary>
public sealed class PdsTestContainer : IAsyncLifetime
{
    private const int PdsPort = 3000;
    private const string PdsHostname = "localhost";

    private IContainer? _container;
    private string? _adminPassword;
    private string? _jwtSecret;

    /// <summary>
    /// Gets the base URL of the PDS instance (e.g., http://localhost:12345)
    /// </summary>
    public string PdsUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the admin password for the PDS instance
    /// </summary>
    public string AdminPassword => _adminPassword ?? throw new InvalidOperationException("Container not initialized");

    /// <summary>
    /// Initializes the PDS container. Called automatically by xUnit.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Generate random secrets for this test instance
        _adminPassword = GenerateRandomPassword();
        _jwtSecret = GenerateRandomSecret();
        var rotationKey = GenerateK256PrivateKey();

        // Get the path to the Dockerfile relative to the test project
        var dockerfileDirectory = GetDockerfileDirectory();

        // Build the custom PDS image from Dockerfile
        var pdsImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "ATProto.Primitives.Tests/Integration/Pds")
            .WithDockerfile("Dockerfile.pds-test")
            .WithName($"pds-test:{Guid.NewGuid():N}")
            .WithCleanUp(true)
            .Build();

        await pdsImage.CreateAsync()
            .ConfigureAwait(false);

        // Create and configure the container
        _container = new ContainerBuilder()
            .WithImage(pdsImage)
            .WithPortBinding(PdsPort, true) // Random host port
            .WithEnvironment("PDS_HOSTNAME", PdsHostname)
            .WithEnvironment("PDS_PORT", PdsPort.ToString())
            .WithEnvironment("PDS_JWT_SECRET", _jwtSecret)
            .WithEnvironment("PDS_ADMIN_PASSWORD", _adminPassword)
            .WithEnvironment("PDS_DATA_DIRECTORY", "/pds")
            .WithEnvironment("PDS_BLOBSTORE_DISK_LOCATION", "/pds/blocks")
            .WithEnvironment("PDS_DID_PLC_URL", "https://plc.directory")
            .WithEnvironment("PDS_BSKY_APP_VIEW_URL", "https://api.bsky.app")
            .WithEnvironment("PDS_BSKY_APP_VIEW_DID", "did:web:api.bsky.app")
            .WithEnvironment("PDS_CRAWLERS", "https://bsky.network")
            .WithEnvironment("PDS_PLC_ROTATION_KEY_K256_PRIVATE_KEY_HEX", rotationKey)
            // Disable service auth requirements for easier testing
            .WithEnvironment("PDS_SERVICE_HANDLE_DOMAINS", ".test")
            .WithEnvironment("LOG_LEVEL", "debug")
            // Just wait for the port to be listening - don't check HTTP yet
            // The PDS takes time to initialize its HTTP server
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(PdsPort))
            .WithCleanUp(true)
            .Build();

        // Start the container
        await _container.StartAsync()
            .ConfigureAwait(false);

        // Set the public URL
        var mappedPort = _container.GetMappedPublicPort(PdsPort);
        PdsUrl = $"http://{_container.Hostname}:{mappedPort}";

        Console.WriteLine($"[PdsTestContainer] Container started. PDS URL: {PdsUrl}");
        Console.WriteLine($"[PdsTestContainer] Waiting for PDS to finish initialization...");

        // Wait for the PDS HTTP server to be ready
        // The port might be open but the HTTP server needs time to initialize
        await WaitForHealthEndpointAsync()
            .ConfigureAwait(false);

        Console.WriteLine($"[PdsTestContainer] PDS is ready!");
    }

    /// <summary>
    /// Creates a test account on the PDS instance.
    /// </summary>
    /// <param name="handle">The handle for the account (e.g., "testuser.test")</param>
    /// <param name="email">Email address for the account</param>
    /// <param name="password">Password for the account</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The DID of the created account</returns>
    public async Task<string> CreateAccountAsync(
        string handle,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (_container == null)
            throw new InvalidOperationException("Container not initialized");

        // Use pdsadmin to create an account
        // Format: pdsadmin account create --handle <handle> --email <email> --password <password>
        var execResult = await _container.ExecAsync(
            new[]
            {
                "pdsadmin",
                "account",
                "create",
                "--handle", handle,
                "--email", email,
                "--password", password
            },
            cancellationToken)
            .ConfigureAwait(false);

        if (execResult.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Failed to create account. Exit code: {execResult.ExitCode}, " +
                $"Stdout: {execResult.Stdout}, Stderr: {execResult.Stderr}");
        }

        // Parse the DID from the output
        // The output typically contains: "Account created: did:plc:xxxxx"
        var did = ParseDidFromOutput(execResult.Stdout);
        return did;
    }

    /// <summary>
    /// Creates an invite code that can be used to create accounts via the API.
    /// </summary>
    public async Task<string> CreateInviteCodeAsync(CancellationToken cancellationToken = default)
    {
        if (_container == null)
            throw new InvalidOperationException("Container not initialized");

        var execResult = await _container.ExecAsync(
            new[] { "pdsadmin", "create-invite-code" },
            cancellationToken)
            .ConfigureAwait(false);

        if (execResult.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Failed to create invite code. Exit code: {execResult.ExitCode}, " +
                $"Stdout: {execResult.Stdout}, Stderr: {execResult.Stderr}");
        }

        return execResult.Stdout.Trim();
    }

    /// <summary>
    /// Gets the container logs for debugging purposes.
    /// </summary>
    public async Task<string> GetLogsAsync(CancellationToken cancellationToken = default)
    {
        if (_container == null)
            throw new InvalidOperationException("Container not initialized");

        var (stdout, stderr) = await _container.GetLogsAsync(ct: cancellationToken)
            .ConfigureAwait(false);

        return $"STDOUT:\n{stdout}\n\nSTDERR:\n{stderr}";
    }

    /// <summary>
    /// Disposes the container. Called automatically by xUnit.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync()
                .ConfigureAwait(false);
        }
    }

    private static string GetDockerfileDirectory()
    {
        // Get the assembly location (e.g., bin/Debug/net8.0)
        var assemblyLocation = typeof(PdsTestContainer).Assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation)!;

        // Navigate up from bin/Debug/net8.0 to the project root
        // Then into Integration/Pds where the Dockerfile lives
        var dockerfileDir = Path.Combine(
            assemblyDirectory,
            "..", "..", "..",  // Up to project root
            "Integration", "Pds");

        var fullPath = Path.GetFullPath(dockerfileDir);

        // Verify the directory exists
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException(
                $"Dockerfile directory not found at: {fullPath}. " +
                $"Assembly location: {assemblyLocation}");
        }

        return fullPath;
    }

    private static string GenerateRandomPassword()
    {
        return Guid.NewGuid().ToString("N");
    }

    private static string GenerateRandomSecret()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string GenerateK256PrivateKey()
    {
        // Generate a random 32-byte private key for secp256k1
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ParseDidFromOutput(string output)
    {
        // Look for DID in the output
        // Format: "Account created: did:plc:xxxxx" or similar
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var didMatch = System.Text.RegularExpressions.Regex.Match(
                line,
                @"(did:plc:[a-z2-7]{24}|did:web:[a-zA-Z0-9\.\-]+)");

            if (didMatch.Success)
            {
                return didMatch.Value;
            }
        }

        throw new InvalidOperationException(
            $"Could not parse DID from pdsadmin output: {output}");
    }

    private async Task WaitForHealthEndpointAsync()
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var healthUrl = $"{PdsUrl}/xrpc/_health";

        // Try for up to 120 seconds (2 minutes - PDS can be slow to start on first run)
        var maxAttempts = 120;
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await httpClient.GetAsync(healthUrl).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"[PdsTestContainer] ✓ Health check passed after {i + 1} seconds");
                    return; // Success!
                }
                else if (i % 10 == 0)
                {
                    Console.WriteLine($"[PdsTestContainer] Attempt {i + 1}/{maxAttempts}: HTTP {(int)response.StatusCode}");
                }
            }
            catch (Exception ex) when (i % 10 == 0)
            {
                // Log every 10 seconds
                Console.WriteLine($"[PdsTestContainer] Attempt {i + 1}/{maxAttempts}: {ex.GetType().Name}: {ex.Message}");
            }
            catch
            {
                // Silently retry for other attempts
            }

            await Task.Delay(1000).ConfigureAwait(false);
        }

        // If we get here, dump logs for debugging
        var (stdout, stderr) = await _container!.GetLogsAsync().ConfigureAwait(false);
        throw new TimeoutException(
            $"PDS health endpoint did not become available within {maxAttempts} seconds.\n" +
            $"URL: {healthUrl}\n" +
            $"Container Logs (stdout):\n{stdout}\n" +
            $"Container Logs (stderr):\n{stderr}");
    }
}
