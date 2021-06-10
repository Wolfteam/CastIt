using Microsoft.AspNetCore.Builder;

namespace CastIt.Server.Common.Extensions
{
    public static class SwaggerAppBuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            string apiName,
            string version = "V1")
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // To serve the Swagger UI at the app's root (http://localhost:<random_port>/)
                c.SwaggerEndpoint($"../swagger/{version}/swagger.json", $"{apiName} {version}");
                c.DocumentTitle = $"{apiName} API";
            });

            return app;
        }
    }

}
