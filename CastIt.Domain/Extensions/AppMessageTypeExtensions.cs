using CastIt.Domain.Enums;
using System;

namespace CastIt.Domain.Extensions
{
    public static class AppMessageTypeExtensions
    {
        public static string GetErrorMsg(this AppMessageType type)
        {
            return type switch
            {
                AppMessageType.UnknownErrorOccurred => "Unknown error",
                AppMessageType.InvalidRequest => "Invalid Request",
                AppMessageType.NotFound => "The resource you were looking for was not found",
                AppMessageType.UnknownErrorLoadingFile => "Unknown error loading file",
                AppMessageType.FileIsAlreadyBeingPlayed => "File is already being played",
                AppMessageType.FileNotSupported => "File is not supported",
                AppMessageType.FilesAreNotValid => "The provided files are not valid",
                AppMessageType.NoFilesToBeAdded => "There are no files to be added",
                AppMessageType.UrlNotSupported => "The provided Url is not supported",
                AppMessageType.UrlCouldntBeParsed => "Url couldn't be parsed",
                AppMessageType.OneOrMoreFilesAreNotReadyYet => "One or more files are not ready yet",
                AppMessageType.NoDevicesFound => "No devices were found",
                AppMessageType.NoInternetConnection => "There's no internet connection",
                AppMessageType.ConnectionToDeviceIsStillInProgress => "Connection to device is still in progress",
                AppMessageType.PlayListNotFound => "Playlist was not found",
                AppMessageType.FileNotFound => "File was not found",
                AppMessageType.FFmpegError => "Unknown error on FFmpeg",
                AppMessageType.ServerIsClosing => "Server is closing",
                AppMessageType.FFmpegExecutableNotFound => "Ffmpeg executable is not valid or was not found",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"The provided type = {type} is not valid")
            };
        }

        public static string GetErrorCode(this AppMessageType msg)
        {
            int msgId = (int)msg;
            return msgId.ToString();
        }
    }
}
