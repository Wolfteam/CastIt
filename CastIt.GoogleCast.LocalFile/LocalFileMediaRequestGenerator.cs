using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.FFmpeg;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Media;
using CastIt.GoogleCast.Models.Play;
using CastIt.GoogleCast.Shared.Enums;
using CastIt.Server.Shared;
using CastIt.Shared.Extensions;
using CastIt.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.LocalFile
{
    public class LocalFileMediaRequestGenerator : IPlayMediaRequestGenerator
    {
        private readonly ILogger<LocalFileMediaRequestGenerator> _logger;
        private readonly IBaseServerService _server;
        private readonly IFFmpegService _ffmpeg;

        public string Identifier
            => nameof(LocalFileMediaRequestGenerator);

        public LocalFileMediaRequestGenerator(
            ILogger<LocalFileMediaRequestGenerator> logger,
            IBaseServerService server,
            IFFmpegService ffmpeg)
        {
            _logger = logger;
            _server = server;
            _ffmpeg = ffmpeg;
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
            _logger.LogInformation($"{nameof(BuildRequest)}: Building request...");

            var fileInfo = await _ffmpeg.GetFileInfo(file.Path, cancellationToken);
            if (fileInfo == null)
            {
                var msg = $"Couldn't retrieve the file info for file = {file.Filename}";
                _logger.LogWarning($"{nameof(BuildRequest)}: {msg}");
                throw new ErrorLoadingFileException(msg);
            }

            file.UpdateFileInfo(fileInfo);
            //TODO: MAYBE MOVE THIS TO A BASE CLASS ?
            if (!fileOptionsChanged)
            {
                _logger.LogInformation($"{nameof(BuildRequest)}: Setting the video and audio streams...");
                file
                    .CleanAllStreams()
                    .SetVideoStreams()
                    .SetAudioStreams();
            }

            bool videoNeedsTranscode = file.Type.IsLocalVideo() && _ffmpeg.VideoNeedsTranscode(
                file.CurrentFileVideoStreamIndex, settings.ForceVideoTranscode,
                settings.VideoScale, file.FileInfo);
            bool audioNeedsTranscode = _ffmpeg.AudioNeedsTranscode(
                file.CurrentFileAudioStreamIndex, settings.ForceAudioTranscode,
                file.FileInfo, file.Type.IsLocalMusic());
            HwAccelDeviceType hwAccelToUse = file.Type.IsLocalVideo()
                ? _ffmpeg.GetHwAccelToUse(file.CurrentFileVideoStreamIndex, file.FileInfo, settings.EnableHardwareAcceleration)
                : HwAccelDeviceType.None;
            string thumbnail = _ffmpeg.GetThumbnail(file.Path);
            string thumbnailUrl = _server.GetChromeCastPreviewUrl(thumbnail);
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
            return BuildRequest(options);
        }

        private PlayMediaRequest BuildRequest(LocalFileGeneratePlayMediaRequest options)
        {
            _logger.LogInformation($"{nameof(BuildRequest)}: File is a local one, generating metadata...");
            string title = Path.GetFileName(options.Mrl);
            var media = new MediaInformation
            {
                Metadata = new MovieMetadata
                {
                    Title = title,
                },
                //You have to set the content type before hand, with that, the album art of a music file will be shown
                ContentType = _server.GetOutputMimeType(options.Mrl),
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

            _logger.LogInformation($"{nameof(BuildRequest)}: Retrieving img url to use...");
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
                ContentType = _server.GetOutputMimeType(options.Mrl)
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

            _logger.LogInformation($"{nameof(BuildRequest)}: Metadata was successfully created");
            return request;
        }
    }
}
