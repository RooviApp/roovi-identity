using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace ATProto.Tests.Integration.Pds;

/// <summary>Real PDS with a local, signature-validating PLC creation/resolution test double.</summary>
public sealed class PdsTestContainer : IAsyncLifetime
{
    // Current PDS distribution (0.4), pinned for reproducible local and CI runs.
    private const string Image = "ghcr.io/bluesky-social/pds@sha256:d95725b24dbe53af9d91dc69750556931ebed6c396f2cfa42b221434db642f12";
    private readonly INetwork _network = new NetworkBuilder().Build();
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };
    private IContainer? _pds;
    private IContainer? _plc;
    public string PdsUrl { get; private set; } = string.Empty;
    public string PlcUrl { get; private set; } = string.Empty;
    // Advertised identity endpoint inside the test environment, distinct from Docker's mapped host URL.
    public string ServiceUrl => "http://localhost:3000";
    public string AdminPassword { get; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    public async ValueTask InitializeAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try
        {
            await _network.CreateAsync(timeout.Token);
            _plc = new ContainerBuilder(Image)
                .WithNetwork(_network)
                .WithNetworkAliases("plc")
                .WithPortBinding(3000, true)
                .WithResourceMapping(Path.Combine(AppContext.BaseDirectory, "Integration", "Pds", "plc.cjs"), "/app/")
                .WithEntrypoint("node", "/app/plc.cjs")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(3000).ForPath("/_health")))
                .Build();
            await _plc.StartAsync(timeout.Token);
            PlcUrl = $"http://{_plc.Hostname}:{_plc.GetMappedPublicPort(3000)}/";

            _pds = new ContainerBuilder(Image)
                .WithNetwork(_network)
                .WithTmpfsMount("/pds")
                .WithPortBinding(3000, true)
                .WithEnvironment("PDS_HOSTNAME", "localhost")
                .WithEnvironment("PDS_PORT", "3000")
                .WithEnvironment("PDS_DEV_MODE", "true")
                .WithEnvironment("PDS_DPOP_SECRET", Convert.ToHexString(RandomNumberGenerator.GetBytes(32)))
                .WithEnvironment("PDS_JWT_SECRET", Convert.ToHexString(RandomNumberGenerator.GetBytes(32)))
                .WithEnvironment("PDS_ADMIN_PASSWORD", AdminPassword)
                .WithEnvironment("PDS_DATA_DIRECTORY", "/pds")
                .WithEnvironment("PDS_BLOBSTORE_DISK_LOCATION", "/pds/blocks")
                .WithEnvironment("PDS_DID_PLC_URL", "http://plc:3000")
                .WithEnvironment("PDS_PLC_ROTATION_KEY_K256_PRIVATE_KEY_HEX", Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant())
                .WithEnvironment("PDS_SERVICE_HANDLE_DOMAINS", ".test")
                .WithEnvironment("PDS_INVITE_REQUIRED", "true")
                .WithEnvironment("PDS_RATE_LIMITS_ENABLED", "false")
                .WithEnvironment("PDS_CRAWLERS", "")
                .WithEnvironment("LOG_LEVEL", "warn")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(3000).ForPath("/xrpc/_health")))
                .Build();
            await _pds.StartAsync(timeout.Token);
            PdsUrl = $"http://{_pds.Hostname}:{_pds.GetMappedPublicPort(3000)}";
        }
        catch (Exception error)
        {
            var logs = await GetLogsAsync();
            await DisposeAsync();
            throw new InvalidOperationException($"PDS fixture startup failed.\n{logs}", error);
        }
    }

    public async Task<string> CreateAccountAsync(string handle, string email, string password, CancellationToken cancellationToken = default)
    {
        var inviteCode = await CreateInviteCodeAsync(cancellationToken);
        using var response = await _httpClient.PostAsJsonAsync($"{PdsUrl}/xrpc/com.atproto.server.createAccount",
            new { handle, email, password, inviteCode }, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Account creation failed ({response.StatusCode}): {await response.Content.ReadAsStringAsync(cancellationToken)}");
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);
        return payload!.RootElement.GetProperty("did").GetString()!;
    }

    public async Task<string> CreateInviteCodeAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{PdsUrl}/xrpc/com.atproto.server.createInviteCode");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"admin:{AdminPassword}")));
        request.Content = JsonContent.Create(new { useCount = 1 });
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);
        return payload!.RootElement.GetProperty("code").GetString()!;
    }

    public async Task<string> GetLogsAsync(CancellationToken cancellationToken = default)
    {
        var logs = new StringBuilder();
        foreach (var container in new[] { _plc, _pds })
        {
            if (container is null) continue;
            try
            {
                var (stdout, stderr) = await container.GetLogsAsync(ct: cancellationToken);
                logs.AppendLine(stdout).AppendLine(stderr);
            }
            catch (Exception error) { logs.AppendLine($"Logs unavailable: {error.Message}"); }
        }
        return logs.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        try { if (_pds is not null) await _pds.DisposeAsync(); }
        finally
        {
            try { if (_plc is not null) await _plc.DisposeAsync(); }
            finally
            {
                await _network.DisposeAsync();
                _httpClient.Dispose();
            }
        }
    }
}
