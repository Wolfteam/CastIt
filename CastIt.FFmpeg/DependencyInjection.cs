using Microsoft.Extensions.DependencyInjection;

namespace CastIt.FFmpeg;

public static class DependencyInjection
{
    public static IServiceCollection AddFFmpeg(this IServiceCollection services)
    {
        services.AddSingleton<IFFmpegService, FFmpegService>();
        return services;
    }
}