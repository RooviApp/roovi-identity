using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RooviApp.Identity.Api;

namespace ATProto.Tests.Integration.Api;

public sealed class ApiSmokeTests
{
    [Fact]
    public async Task Swagger_DescribesTheApiInDevelopment()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        using var response = await client.GetAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var document = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("/api/Authentication/login", document);
    }
}
