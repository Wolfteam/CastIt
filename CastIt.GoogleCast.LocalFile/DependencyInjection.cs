using Microsoft.Extensions.DependencyInjection;

namespace CastIt.GoogleCast.LocalFile;

public static class DependencyInjection
{
    public static IServiceCollection AddGoogleCastLocalFiles(this IServiceCollection services)
    {
        services.AddSingleton<LocalFileMediaRequestGenerator>();
        return services;
    }
}