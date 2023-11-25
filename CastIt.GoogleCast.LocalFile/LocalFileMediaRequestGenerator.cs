using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.FFmpeg;
using CastIt.GoogleCast.Generator;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Media;
using CastIt.GoogleCast.Models.Play;
using CastIt.GoogleCast.Shared.Enums;
using CastIt.Server.Shared;
using CastIt.Shared.Extensions;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.LocalFile
{
    public class LocalFileMediaRequestGenerator : BaseMediaRequestGenerator, IPlayMediaRequestGenerator
    {
        public string Identifier
            => nameof(LocalFileMediaRequestGenerator);

        public LocalFileMediaRequestGenerator(
            ILogger<LocalFileMediaRequestGenerator> logger,
            IBaseServerService server,
            IFFmpegService ffmpeg,
            IFileService fileService)
            : base(logger, ffmpeg, server, fileService)
        {
        }

        public Task<bool> CanHandleRequest(
            string mrl,
            AppFileType type,
            CancellationToken cancellationToken = default)
        {
            bool can = type.IsVideoOrMusic();
            return Task.FromResult(can);
        }

        public async Task<PlayMediaRequest> BuildRequest(
            ServerFileItem file,
            ServerAppSettings settings,
            double seekSeconds,
            bool fileOptionsChanged,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation($"{nameof(BuildRequest)}: Building request...");

            var fileInfo = await FFmpeg.GetFileInfo(file.Path, cancellationToken);
            if (fileInfo == null)
            {
                var msg = $"Couldn't retrieve the file info for file = {file.Filename}";
                Logger.LogWarning($"{nameof(BuildRequest)}: {msg}");
                throw new ErrorLoadingFileException(msg);
            }

            file.UpdateFileInfo(fileInfo);
            SetStreams(file, fileOptionsChanged);

            if (file.Type.IsLocalVideo() && !fileOptionsChanged)
            {
                Logger.LogInformation($"{nameof(BuildRequest)}: Setting the sub streams...");
                var (localSubsPath, filename) = TryGetSubTitlesLocalPath(file.Path);
                file.SetSubtitleStreams(localSubsPath, filename, settings.LoadFirstSubtitleFoundAutomatically);
            }

            bool videoNeedsTranscode = file.Type.IsLocalVideo() && FFmpeg.VideoNeedsTranscode(
                file.CurrentFileVideoStreamIndex, settings.ForceVideoTranscode,
                settings.VideoScale, file.FileInfo);
            bool audioNeedsTranscode = FFmpeg.AudioNeedsTranscode(
                file.CurrentFileAudioStreamIndex, settings.ForceAudioTranscode,
                file.FileInfo, file.Type.IsLocalMusic());
            HwAccelDeviceType hwAccelToUse = file.Type.IsLocalVideo()
                ? FFmpeg.GetHwAccelToUse(file.CurrentFileVideoStreamIndex, file.FileInfo, settings.EnableHardwareAcceleration)
                : HwAccelDeviceType.None;
            string thumbnail = FFmpeg.GetThumbnail(file.Id, file.Path);
            string thumbnailUrl = Server.GetChromeCastPreviewUrl(file.Id);
            var options = new LocalFileGeneratePlayMediaRequest
            {
                FileInfo = file.FileInfo,
                Mrl = file.Path,
                ThumbnailUrl = thumbnailUrl,
                FileType = file.Type,
                VideoNeedsTranscode = videoNeedsTranscode,
                AudioNeedsTranscode = audioNeedsTranscode,
                HwAccel = hwAccelToUse,
                VideoStreamIndex = file.CurrentFileVideoStreamIndex,
                AudioStreamIndex = file.CurrentFileAudioStreamIndex,
                SubTitleStreamIndex = file.CurrentFileSubTitleStreamIndex,
                VideoScale = settings.VideoScale,
                Seconds = seekSeconds,
            };
            var request = BuildRequest(options);
            await SetSubtitlesIfAny(file, settings, request, cancellationToken);

            Logger.LogInformation($"{nameof(BuildRequest)}: Request was built");
            return request;
        }

        public async Task HandleSecondsChanged(
            ServerFileItem file,
            ServerAppSettings settings,
            PlayMediaRequest request,
            double newSeconds,
            CancellationToken cancellationToken = default)
        {
            request.SeekSeconds = newSeconds;
            await SetSubtitlesIfAny(file, settings, request, cancellationToken);
        }

        private PlayMediaRequest BuildRequest(LocalFileGeneratePlayMediaRequest options)
        {
            Logger.LogInformation($"{nameof(BuildRequest)}: File is a local one, generating metadata...");
            string title = Path.GetFileName(options.Mrl);
            var media = new MediaInformation
            {
                Metadata = new MovieMetadata
                {
                    Title = title,
                },
                //You have to set the content type before hand, with that, the album art of a music file will be shown
                ContentType = Server.GetOutputMimeType(options.Mrl),
                Duration = options.FileInfo.Format.Duration
            };

            if (options.FileType.IsLocalVideo())
            {
                media.StreamType = StreamType.Live;
            }
            else if (options.FileType.IsLocalMusic())
            {
                media.Metadata = new MusicTrackMediaMetadata
                {
                    Title = title,
                    AlbumName = options.FileInfo.Format.Tag?.Album,
                    Artist = options.FileInfo.Format.Tag?.Artist,
                };
            }

            Logger.LogInformation($"{nameof(BuildRequest)}: Retrieving img url to use...");
            if (!string.IsNullOrEmpty(options.ThumbnailUrl))
            {
                media.Metadata.Images.Add(new GoogleCast.Models.Image
                {
                    Url = options.ThumbnailUrl
                });
            }

            var playRequest = new PlayAppFileRequestDto
            {
                StreamUrls = new List<string>
                {
                    options.Mrl,
                },
                VideoStreamIndex = options.VideoStreamIndex,
                AudioStreamIndex = options.AudioStreamIndex,
                Seconds = options.Seconds,
                VideoNeedsTranscode = options.VideoNeedsTranscode,
                AudioNeedsTranscode = options.AudioNeedsTranscode,
                HwAccelToUse = options.HwAccel,
                VideoScale = options.VideoScale,
                SelectedQuality = -1,
                VideoWidthAndHeight = options.FileInfo.Videos
                    .Find(f => f.Index == options.VideoStreamIndex)?
                    .WidthAndHeightText,
                ContentType = Server.GetOutputMimeType(options.Mrl)
            };

            var request = new PlayMediaRequest
            {
                VideoStreamIndex = options.VideoStreamIndex,
                AudioStreamIndex = options.AudioStreamIndex,
                MediaInfo = media,
                FileInfo = options.FileInfo,
                SeekSeconds = options.Seconds,
                SubtitleStreamIndex = options.SubTitleStreamIndex,
                Base64 = playRequest.ToBase64(),
                IsHandledByServer = true,
                OriginalMrl = options.Mrl
            };

            Logger.LogInformation($"{nameof(BuildRequest)}: Metadata was successfully created");
            return request;
        }

        private (string, string) TryGetSubTitlesLocalPath(string mrl)
        {
            Logger.LogInformation(
                $"{nameof(TryGetSubTitlesLocalPath)}: Checking if subtitle exist in the same " +
                $"dir as file = {mrl}");
            var (possibleSubTitlePath, filename) = FileService.TryGetSubTitlesLocalPath(mrl);
            if (!string.IsNullOrWhiteSpace(possibleSubTitlePath))
            {
                Logger.LogInformation(
                    $"{nameof(TryGetSubTitlesLocalPath)}: Found subtitles in " +
                    $"path = {possibleSubTitlePath}");
                return (possibleSubTitlePath, filename);
            }

            Logger.LogInformation(
                $"{nameof(TryGetSubTitlesLocalPath)}: No subtitles were found " +
                $"for file = {mrl}");
            return (possibleSubTitlePath, filename);
        }
    }
}
