using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;

namespace CastIt.Test.Common.Extensions
{
    public static class SwaggerIdentityServiceCollections
    {
        public static IServiceCollection AddSwagger(
            this IServiceCollection services,
            string apiName,
            string xmlFileName,
            string version = "V1")
        {
            var apiInfo = new OpenApiInfo
            {
                Version = version,
                Title = $"{apiName} api",
                Description = $"This is the {apiName} api"
            };

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(version, apiInfo);
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddSwaggerGenNewtonsoftSupport();
            return services;
        }
    }
}
