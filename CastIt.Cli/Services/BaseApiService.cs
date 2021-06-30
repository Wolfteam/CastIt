using CastIt.Domain.Dtos;
using CastIt.Domain.Enums;
using CastIt.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Cli.Services
{
    public abstract class BaseApiService
    {
        protected readonly ILogger Logger;
        protected BaseApiService(ILogger logger)
        {
            Logger = logger;
        }

        protected async Task HandleApiException<T>(
            ApiException ex,
            List<T> responses,
            AppMessageType defaultError = AppMessageType.UnknownErrorOccurred)
            where T : EmptyResponseDto
        {
            foreach (var response in responses)
            {
                await HandleApiException(ex, response, defaultError);
            }
        }

        protected async Task HandleApiException<T>(
           ApiException ex,
           T response,
           AppMessageType defaultError = AppMessageType.UnknownErrorOccurred)
           where T : EmptyResponseDto
        {
            var error = await ex.GetContentAsAsync<EmptyResponseDto>();
            //If for some reason, we cant get an error response, lets set a default one
            if (error is null)
            {
                Logger.LogError(ex,
                    $"{nameof(HandleApiException)}: Response doesn't have a body, " +
                    $"so this may be an error produced by this app");
                HandleUnknownException(response, defaultError);
            }
            else
            {
                Logger.LogError(ex,
                    $"{nameof(HandleApiException)}: Response does have a body, " +
                    $"Error = {error.Message} - {error.MessageId}");
                response.Message = error.Message;
                response.MessageId = error.MessageId;
            }

#if DEBUG
            response.Message += Environment.NewLine + ex;
#endif
        }

        protected void HandleUnknownException<T>(
            T response,
            AppMessageType defaultError = AppMessageType.UnknownErrorOccurred)
            where T : EmptyResponseDto
        {
            response.MessageId = defaultError.GetErrorCode();
            response.Message = defaultError.GetErrorMsg();
        }
    }
}
