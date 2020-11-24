using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Domain.Enums;
using CastIt.Domain.Models.FFmpeg.Transcode;
using EmbedIO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Server.Modules
{
    internal class MediaModule : WebModuleBase
    {
        private readonly ILogger _logger;
        private readonly IFFmpegService _ffmpegService;
        private readonly ITelemetryService _telemetryService;
        private readonly IFileService _fileService;

        private CancellationTokenSource _tokenSource;
        private bool _checkTranscodeProcess;

        public MediaModule(
            ILogger logger,
            IFFmpegService fFMpegService,
            ITelemetryService telemetryService,
            IFileService fileService,
            string baseRoute)
            : base(baseRoute)
        {
            _logger = logger;
            _ffmpegService = fFMpegService;
            _telemetryService = telemetryService;
            _fileService = fileService;
            _tokenSource = new CancellationTokenSource();
        }

        public override bool IsFinalHandler => false;

        protected override async Task OnRequestAsync(IHttpContext context)
        {
            var query = context.GetRequestQueryData();
            if (query.Count == 0 || !query.AllKeys.All(q => AppWebServerConstants.AllowedQueryParameters.Contains(q)))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.StatusDescription = "You need to provide all the query params";
                context.SetHandled();
                _logger.LogWarning($"{nameof(OnRequestAsync)}: Query params does not contain the all the allowed values.");
                return;
            }
            try
            {
                string filepath = query[AppWebServerConstants.FileQueryParameter];
                bool isVideoFile = _fileService.IsVideoFile(filepath);
                bool isMusicFile = _fileService.IsMusicFile(filepath);
                bool isHls = _fileService.IsHls(filepath);

                if (!isVideoFile && !isMusicFile && !isHls)
                {
                    _logger.LogWarning($"{nameof(OnRequestAsync)}: File = {filepath} is not a video nor music file.");
                    return;
                }

                context.Response.ContentType = _ffmpegService.GetOutputTranscodeMimeType(filepath);

                context.Response.DisableCaching();

                if (_checkTranscodeProcess)
                {
                    _tokenSource.Cancel();
                    _tokenSource = new CancellationTokenSource();
                    _ffmpegService.KillTranscodeProcess();
                }

                _checkTranscodeProcess = true;
                if (isVideoFile || isHls)
                {
                    var options = GetVideoFileOptions(filepath, query);
                    _logger.LogInformation($"{nameof(OnRequestAsync)}: Handling request for video file with options = {JsonConvert.SerializeObject(options)}");
                    await _ffmpegService.TranscodeVideo(context.Response.OutputStream, options, _tokenSource.Token).ConfigureAwait(false);
                }
                else
                {
                    var options = GetMusicFileOptions(filepath, query);
                    _logger.LogInformation($"{nameof(OnRequestAsync)}: Handling request for music file with options = {JsonConvert.SerializeObject(options)}");
                    await using var memoryStream = await _ffmpegService.TranscodeMusic(options, _tokenSource.Token).ConfigureAwait(false);
                    //TODO: THIS LENGTH IS NOT WORKING PROPERLY
                    context.Response.ContentLength64 = memoryStream.Length;
                    await memoryStream.CopyToAsync(context.Response.OutputStream, _tokenSource.Token)
                        .ConfigureAwait(false);
                }
                _logger.LogInformation($"{nameof(OnRequestAsync)}: Request was successfully handled for file = {filepath}.");
            }
            catch (Exception e)
            {
                if (e is IOException || e is TaskCanceledException)
                    return;
                _logger.LogError(e, $"{nameof(OnRequestAsync)}: Unknown error occurred");
                _telemetryService.TrackError(e);
            }
            finally
            {
                context.SetHandled();
            }
        }

        private static TranscodeVideoFile GetVideoFileOptions(string filepath, NameValueCollection query)
        {
            double seconds = double.Parse(query[AppWebServerConstants.SecondsQueryParameter]!);
            int videoStreamIndex = int.Parse(query[AppWebServerConstants.VideoStreamIndexParameter]!);
            int audioStreamIndex = int.Parse(query[AppWebServerConstants.AudioStreamIndexParameter]!);
            bool videoNeedsTranscode = bool.Parse(query[AppWebServerConstants.VideoNeedsTranscode]!);
            bool audioNeedsTranscode = bool.Parse(query[AppWebServerConstants.AudioNeedsTranscode]!);
            var hwAccelType = Enum.Parse<HwAccelDeviceType>(query[AppWebServerConstants.HwAccelTypeToUse]!, true);
            string videoWidthAndHeight = query[AppWebServerConstants.VideoWidthAndHeight];
            return new TranscodeVideoFileBuilder()
                .WithDefaults(hwAccelType, VideoScaleType.Original, videoWidthAndHeight)
                .WithStreams(videoStreamIndex, audioStreamIndex)
                .WithFile(filepath)
                .ForceTranscode(videoNeedsTranscode, audioNeedsTranscode)
                .GoTo(seconds)
                .Build();
        }

        private static TranscodeMusicFile GetMusicFileOptions(string filepath, NameValueCollection query)
        {
            double seconds = double.Parse(query[AppWebServerConstants.SecondsQueryParameter]!);
            int audioStreamIndex = int.Parse(query[AppWebServerConstants.AudioStreamIndexParameter]!);
            bool audioNeedsTranscode = bool.Parse(query[AppWebServerConstants.AudioNeedsTranscode]!);
            return new TranscodeMusicFileBuilder()
                .WithAudio(audioStreamIndex)
                .WithFile(filepath)
                .ForceTranscode(false, audioNeedsTranscode)
                .GoTo(seconds)
                .Build();
        }
    }
}
