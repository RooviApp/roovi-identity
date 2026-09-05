using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RooviApp.Identity.Api;

namespace ATProto.Tests.Integration;

/// <summary>
/// Base class for integration tests that provides a WebApplicationFactory
/// and common test infrastructure for testing the API with real HTTP requests.
/// </summary>
public class BaseIntegrationTest(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory = factory;
    protected readonly HttpClient Client = factory.CreateClient();

  /// <summary>
  /// Creates a custom client with specific service overrides.
  /// </summary>
  protected HttpClient CreateClientWithServices(Action<IServiceCollection> configureServices)
    {
        var customFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(configureServices);
        });

        return customFactory.CreateClient();
    }

    /// <summary>
    /// Gets a service from the test server's service provider.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}
