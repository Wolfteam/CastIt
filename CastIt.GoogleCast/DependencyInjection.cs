using CastIt.GoogleCast.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CastIt.GoogleCast;

public static class DependencyInjection
{
    public static IServiceCollection AddGoogleCast(this IServiceCollection services, bool useDummyPlayer)
    {
        return useDummyPlayer
            ? services.AddDummyGoogleCast()
            : services.AddSingleton<IPlayer>(provider => new Player(provider.GetRequiredService<ILogger<Player>>()));
    }

    public static IServiceCollection AddDummyGoogleCast(this IServiceCollection services)
    {
        return services.AddSingleton<IPlayer>(new DummyPlayer());
    }
}