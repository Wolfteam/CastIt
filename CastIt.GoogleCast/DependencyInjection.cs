using CastIt.GoogleCast.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CastIt.GoogleCast
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGoogleCast(this IServiceCollection services)
        {
            return services.AddSingleton<IPlayer>(provider => new Player(provider.GetRequiredService<ILogger<Player>>()));
        }
    }
}
