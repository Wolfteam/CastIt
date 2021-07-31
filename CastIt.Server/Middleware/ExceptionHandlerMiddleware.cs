using CastIt.Domain.Dtos;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
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

        public async Task Invoke(HttpContext context, ILogger<ExceptionHandlerMiddleware> logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(context, e, logger);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
        {
            logger.LogInformation($"{nameof(HandleExceptionAsync)}: Handling exception of type = {exception.GetType()}....");

            var code = HttpStatusCode.InternalServerError;

            var response = new EmptyResponseDto
            {
                Message = AppMessageType.UnknownErrorOccurred.GetErrorMsg(),
                MessageId = AppMessageType.UnknownErrorOccurred.GetErrorMsg(),
            };
            //TODO: SEND CAST MSG
            context.Response.ContentType = "application/json";
            switch (exception)
            {
                case PlayListNotFoundException notFoundEx:
                    code = HttpStatusCode.NotFound;
                    response.MessageId = notFoundEx.ErrorMessageId.GetErrorCode();
                    response.Message = notFoundEx.Message;
                    //castService.SendPlayListNotFound();
                    break;
                case FileNotFoundException notFoundEx:
                    code = HttpStatusCode.NotFound;
                    response.MessageId = notFoundEx.ErrorMessageId.GetErrorCode();
                    response.Message = notFoundEx.Message;
                    //castService.SendFileNotFound();
                    break;
                case InvalidRequestException invEx:
                    code = HttpStatusCode.BadRequest;
                    response.MessageId = invEx.ErrorMessageId.GetErrorCode();
                    response.Message = invEx.Message;
                    //castService.SendInvalidRequest();
                    break;
                default:
                    logger.LogError(exception, $"{nameof(HandleExceptionAsync)}: Unknown exception was captured");
                    break;
            }
#if DEBUG
            response.Message += $". Ex: {exception}";
#endif
            context.Response.StatusCode = (int)code;

            logger.LogInformation(
                $"{nameof(HandleExceptionAsync)}: The final response is going to " +
                $"be = {response.MessageId} - {response.Message}");

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response, SerializerSettings));
        }
    }

}
