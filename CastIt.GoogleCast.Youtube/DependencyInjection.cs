using CastIt.Youtube;
using Microsoft.Extensions.DependencyInjection;

namespace CastIt.GoogleCast.Youtube;

public static class DependencyInjection
{
    public static IServiceCollection AddGoogleCastYoutube(this IServiceCollection services)
    {
        return services.AddYoutubeParser().AddYoutubeMediaRequestGenerator();
    }

    private static IServiceCollection AddYoutubeMediaRequestGenerator(this IServiceCollection services)
    {
        services.AddSingleton<YoutubeMediaRequestGenerator>();
        return services;
    }
}