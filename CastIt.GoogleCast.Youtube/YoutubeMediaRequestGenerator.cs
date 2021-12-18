using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.FFmpeg;
using CastIt.GoogleCast.Generator;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models;
using CastIt.GoogleCast.Models.Media;
using CastIt.GoogleCast.Models.Play;
using CastIt.GoogleCast.Shared.Enums;
using CastIt.Server.Shared;
using CastIt.Shared.Extensions;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Models;
using CastIt.Youtube;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Youtube
{
    public class YoutubeMediaRequestGenerator : BaseMediaRequestGenerator, IPlayMediaRequestGenerator
    {
        private readonly IYoutubeUrlDecoder _decoder;

        public string Identifier
            => nameof(YoutubeMediaRequestGenerator);

        public YoutubeMediaRequestGenerator(
            ILogger<YoutubeMediaRequestGenerator> logger,
            IYoutubeUrlDecoder decoder,
            IBaseServerService server,
            IFFmpegService ffmpeg,
            IFileService fileService)
            : base(logger, ffmpeg, server, fileService)
        {
            _decoder = decoder;
        }

        public Task<bool> CanHandleRequest(
            string mrl,
            AppFileType type,
            CancellationToken cancellationToken = default)
        {
            bool can = type.IsUrl() && _decoder.IsYoutubeUrl(mrl);
            return Task.FromResult(can);
        }

        public async Task<PlayMediaRequest> BuildRequest(
            ServerFileItem file,
            ServerAppSettings settings,
            double seekSeconds,
            bool fileOptionsChanged,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation($"{nameof(BuildRequest)}: Building play request from yt url = {file.Path} ...");
            if (!_decoder.IsYoutubeUrl(file.Path))
            {
                var msg = $"Url = {file.Path} is not supported";
                Logger.LogInformation($"{nameof(BuildRequest)}: {msg}");
                throw new FileNotSupportedException(msg);
            }
            var basicInfo = await _decoder.ParseBasicInfo(file.Path, cancellationToken);
            int desiredQuality = fileOptionsChanged ? file.CurrentFileQuality : (int)settings.WebVideoQuality;
            var ytMedia = await _decoder.Parse(basicInfo, desiredQuality, cancellationToken);
            if (ytMedia.IsHls)
            {
                return await BuildRequestFromHls(
                    ytMedia, file, settings, seekSeconds,
                    fileOptionsChanged, desiredQuality, cancellationToken);
            }

            if (ytMedia.IsFromAdaptiveFormat)
            {
                return await BuildRequestFromAdaptiveFormats(
                    ytMedia, file, settings,
                    seekSeconds, fileOptionsChanged, cancellationToken);
            }

            return await BuildRequestFromFormats(
                ytMedia, file, settings,
                seekSeconds, fileOptionsChanged, cancellationToken);
        }

        private async Task<MediaInformation> BuildBasicMediaInformation(
            long fileId,
            YoutubeMedia ytMedia)
        {
            //TODO: SHOULD I SET THE STREAM TYPE ?
            var media = new MediaInformation
            {
                ContentId = ytMedia.VideoUrl,
                Metadata = new MovieMetadata
                {
                    Title = ytMedia.Title,
                    Subtitle = ytMedia.Description
                },
                //You have to set the content type before hand, with that, the album art of a music file will be shown
                ContentType = "video/webm"
            };

            if (!string.IsNullOrEmpty(ytMedia.ThumbnailUrl))
            {
                string newUrl = await CacheThumbnailImage(fileId, ytMedia.ThumbnailUrl);
                media.Metadata.Images.Add(new Image
                {
                    Url = newUrl
                });
            }

            return media;
        }

        private async Task<string> CacheThumbnailImage(long fileId, string url)
        {
            var path = await FileService.DownloadAndSavePreviewImage(fileId, url);
            return Server.GetChromeCastPreviewUrl(path);
        }

        private async Task<YoutubePlayMediaRequest> BuildRequestFromFormats(
            YoutubeMedia ytMedia,
            ServerFileItem file,
            ServerAppSettings settings,
            double seekSeconds,
            bool fileOptionsChanged,
            CancellationToken cancellationToken)
        {
            //can't set the fileInfo cause this one will be handled by the chromecast
            file.UpdateFileInfo(new FFProbeFileInfo
            {
                Format = new FileInfoFormat()
            });
            SetStreams(file, fileOptionsChanged);
            var request = new YoutubePlayMediaRequest
            {
                VideoQuality = file.CurrentFileQuality,
                FileInfo = file.FileInfo,
                SelectedQuality = ytMedia.SelectedQuality,
                Qualities = ytMedia.Qualities,
                SubtitleStreamIndex = -1,
                OriginalMrl = file.Path,
                SeekSeconds = seekSeconds,
                MediaInfo = await BuildBasicMediaInformation(file.Id, ytMedia),
            };

            await SetSubtitlesIfAny(file, settings, request, cancellationToken);
            file.SetQualitiesStreams(ytMedia.SelectedQuality, ytMedia.Qualities);
            return request;
        }

        private async Task<YoutubePlayMediaRequest> BuildRequestFromAdaptiveFormats(
            YoutubeMedia ytMedia,
            ServerFileItem file,
            ServerAppSettings settings,
            double seekSeconds,
            bool fileOptionsChanged,
            CancellationToken cancellationToken)
        {
            Logger.LogInformation($"{nameof(BuildRequestFromAdaptiveFormats)}: Retrieving file info from adaptive formats...");
            string url = ytMedia.AdaptiveFormatUrls.First();
            var fileInfo = await FFmpeg.GetFileInfo(url, cancellationToken);
            if (fileInfo == null)
            {
                Logger.LogWarning($"{nameof(BuildRequestFromAdaptiveFormats)}: Couldn't get the file info for url = {url}");
                throw new Exception($"File info is null for yt hls = {url}");
            }
            file.UpdateFileInfo(fileInfo);
            SetStreams(file, fileOptionsChanged);

            HwAccelDeviceType hwAccelToUse = FFmpeg.GetHwAccelToUse(
                file.CurrentFileVideoStreamIndex,
                fileInfo,
                settings.EnableHardwareAcceleration);
            var playRequest = new PlayAppFileRequestDto
            {
                StreamUrls = ytMedia.AdaptiveFormatUrls,
                Seconds = seekSeconds,
                //TODO: DOES IT NEEDS TRANSCODE ?
                VideoNeedsTranscode = false,
                AudioNeedsTranscode = false,
                HwAccelToUse = hwAccelToUse,
                VideoScale = settings.VideoScale,
                SelectedQuality = ytMedia.SelectedQuality,
                AudioStreamIndex = -1,
                VideoStreamIndex = -1,
                ContentType = Server.GetOutputMimeType(ytMedia.AdaptiveFormatUrls.First())
            };
            var request = new YoutubePlayMediaRequest
            {
                VideoQuality = file.CurrentFileQuality,
                FileInfo = file.FileInfo,
                SelectedQuality = ytMedia.SelectedQuality,
                Qualities = ytMedia.Qualities,
                SubtitleStreamIndex = -1,
                OriginalMrl = file.Path,
                MediaInfo = await BuildBasicMediaInformation(file.Id, ytMedia),
                IsHandledByServer = true,
                UsesAdaptiveFormats = true,
                Base64 = playRequest.ToBase64()
            };
            request.MediaInfo.Duration = fileInfo.Format?.Duration ?? -1;
            await SetSubtitlesIfAny(file, settings, request, cancellationToken);
            file.SetQualitiesStreams(ytMedia.SelectedQuality, ytMedia.Qualities);
            return request;
        }

        private async Task<YoutubePlayMediaRequest> BuildRequestFromHls(
            YoutubeMedia ytMedia,
            ServerFileItem file,
            ServerAppSettings settings,
            double seekSeconds,
            bool fileOptionsChanged,
            int desiredQuality,
            CancellationToken cancellationToken)
        {
            Logger.LogInformation($"{nameof(BuildRequestFromHls)}: Url is a yt hls, getting its file info...");
            var fileInfo = await FFmpeg.GetFileInfo(ytMedia.Url, cancellationToken);
            if (fileInfo == null)
            {
                Logger.LogWarning($"{nameof(BuildRequestFromHls)}: Couldn't get the file info for url = {ytMedia.Url}");
                throw new Exception($"File info is null for yt hls = {ytMedia.Url}");
            }
            file.UpdateFileInfo(fileInfo);
            SetStreams(file, fileOptionsChanged);

            int closestQuality = fileInfo.HlsVideos
                .Select(v => v.Height)
                .GetClosest(desiredQuality);

            Logger.LogInformation($"{nameof(BuildRequestFromHls)}: The quality to use for this hls will be = {closestQuality}...");
            var videoInfo = fileInfo.HlsVideos.First(v => v.Height == closestQuality);
            int videoStreamIndex = videoInfo.Index;
            int audioStreamIndex = fileInfo.Audios.Any()
                ? fileInfo.Audios.Select(a => a.Index).GetClosest(videoStreamIndex)
                : -1;

            bool videoNeedsTranscode = FFmpeg.VideoNeedsTranscode(
                videoStreamIndex, settings.ForceVideoTranscode,
                settings.VideoScale, fileInfo);
            bool audioNeedsTranscode = FFmpeg.AudioNeedsTranscode(
                audioStreamIndex, settings.ForceAudioTranscode, fileInfo);

            Logger.LogInformation($"{nameof(BuildRequestFromHls)}: The quality to use for this hls will be = {closestQuality}...");
            var playRequest = new PlayAppFileRequestDto
            {
                StreamUrls = new List<string>
                {
                    ytMedia.Url
                },
                VideoStreamIndex = videoStreamIndex,
                AudioStreamIndex = audioStreamIndex,
                Seconds = seekSeconds,
                VideoNeedsTranscode = videoNeedsTranscode,
                AudioNeedsTranscode = audioNeedsTranscode,
                //TODO NONE ?
                HwAccelToUse = HwAccelDeviceType.None,
                VideoScale = settings.VideoScale,
                SelectedQuality = closestQuality,
                VideoWidthAndHeight = fileInfo.HlsVideos
                    .First(v => v.Height == closestQuality)?
                    .WidthAndHeightText,
                ContentType = Server.GetOutputMimeType(ytMedia.Url)
            };
            var media = await BuildBasicMediaInformation(file.Id, ytMedia);
            media.Duration = -1;
            media.StreamType = StreamType.Live;
            media.ContentType = playRequest.ContentType;
            var request = new YoutubePlayMediaRequest
            {
                MediaInfo = media,
                VideoQuality = closestQuality,
                FileInfo = fileInfo,
                SelectedQuality = closestQuality,
                Qualities = ytMedia.Qualities,
                SubtitleStreamIndex = -1,
                OriginalMrl = file.Path,
                IsHandledByServer = true,
                Base64 = playRequest.ToBase64(),
                VideoStreamIndex = videoStreamIndex,
                AudioStreamIndex = audioStreamIndex,
            };
            await SetSubtitlesIfAny(file, settings, request, cancellationToken);
            file.SetQualitiesStreams(closestQuality, ytMedia.Qualities);
            return request;
        }
    }
}