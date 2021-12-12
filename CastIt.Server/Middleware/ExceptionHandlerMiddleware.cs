using CastIt.Server.Common.Extensions;
using CastIt.Server.Interfaces;
using CastIt.Shared.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace CastIt.Server.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext context,
            IServerCastService castService,
            ILogger<ExceptionHandlerMiddleware> logger,
            ITelemetryService telemetryService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(context, e, castService, logger, telemetryService);
            }
        }

        private Task HandleExceptionAsync(
            HttpContext context,
            Exception exception,
            ICastService castService,
            ILogger logger,
            ITelemetryService telemetryService)
        {
            logger.LogInformation($"{nameof(HandleExceptionAsync)}: Handling exception of type = {exception.GetType()}....");
            var response = exception.GenerateResponse(context, castService, telemetryService);

            logger.LogInformation(
                $"{nameof(HandleExceptionAsync)}: The final response is going to " +
                $"be = {response.MessageId} - {response.Message}");

            telemetryService.TrackError(exception);
            return context.Response.WriteAsync(JsonConvert.SerializeObject(response, SerializerSettings));
        }
    }

}
