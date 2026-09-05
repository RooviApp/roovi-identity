using ATProto.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ATProto.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddATProto(this IServiceCollection services)
    {
        services.AddSingleton<IDIDResolver, DIDResolver>();
        services.AddSingleton<IdentityResolver>();
        return services;
    }
}
