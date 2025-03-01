using Microsoft.Extensions.DependencyInjection;

namespace CastIt.Youtube;

public static class DependencyInjection
{
    public static IServiceCollection AddYoutubeParser(this IServiceCollection services)
    {
        services.AddSingleton<IYoutubeUrlDecoder, YoutubeUrlDecoder>();
        return services;
    }
}