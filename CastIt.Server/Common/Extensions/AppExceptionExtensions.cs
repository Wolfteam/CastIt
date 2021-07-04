using CastIt.Application.Interfaces;
using CastIt.Domain.Dtos;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;

namespace CastIt.Server.Common.Extensions
{
    public static class AppExceptionExtensions
    {
        public static EmptyResponseDto GenerateResponse(
            this Exception exception,
            HttpContext context,
            ICastService castService,
            ITelemetryService telemetryService)
        {
            var code = exception switch
            {
                FileNotFoundException _ => HttpStatusCode.NotFound,
                FileNotReadyException _ => HttpStatusCode.BadRequest,
                FileNotSupportedException _ => HttpStatusCode.BadRequest,
                FFmpegInvalidExecutable _ => HttpStatusCode.BadRequest,
                InvalidRequestException _ => HttpStatusCode.BadRequest,
                NoDevicesException _ => HttpStatusCode.BadRequest,
                PlayListNotFoundException _ => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            var response = new EmptyResponseDto
            {
                Message = AppMessageType.UnknownErrorOccurred.GetErrorMsg(),
                MessageId = AppMessageType.UnknownErrorOccurred.GetErrorMsg(),
            };
            exception.HandleCastException(castService, telemetryService);
            if (exception is BaseAppException baseAppException)
            {
                response.MessageId = baseAppException.ErrorMessageId.GetErrorCode();
                response.Message = baseAppException.Message;
            }
#if DEBUG
            response.Message += $". Ex: {exception}";
#endif
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return response;
        }

        public static void HandleCastException(
            this Exception e,
            ICastService castService,
            ITelemetryService telemetryService)
        {
            switch (e)
            {
                case ConnectingException _:
                    castService.SendServerMsg(AppMessageType.ConnectionToDeviceIsStillInProgress);
                    break;
                case ErrorLoadingFileException _:
                    castService.SendErrorLoadingFile();
                    break;
                case FFmpegException _:
                    castService.SendServerMsg(AppMessageType.FFmpegError);
                    break;
                case FFmpegInvalidExecutable _:
                    castService.SendServerMsg(AppMessageType.FFmpegExecutableNotFound);
                    break;
                case FileNotFoundException _:
                    castService.SendFileNotFound();
                    break;
                case FileNotReadyException _:
                    castService.SendServerMsg(AppMessageType.OneOrMoreFilesAreNotReadyYet);
                    break;
                case FileNotSupportedException _:
                    castService.SendServerMsg(AppMessageType.FileNotSupported);
                    break;
                case InvalidRequestException _:
                    castService.SendInvalidRequest();
                    break;
                case NoDevicesException _:
                    castService.SendNoDevicesFound();
                    break;
                case PlayListNotFoundException _:
                    castService.SendPlayListNotFound();
                    break;
                default:
                    telemetryService.TrackError(e);
                    castService.SendServerMsg(AppMessageType.UnknownErrorOccurred);
                    break;
            }
        }
    }
}
