using CastIt.Common.Enums;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using EmbedIO;
using MvvmCross.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Server
{
    internal class MediaModule : WebModuleBase
    {
        private readonly IMvxLog _logger;
        private readonly IFFMpegService _ffmpegService;
        private readonly ITelemetryService _telemetryService;

        private CancellationTokenSource _tokenSource;
        private bool _checkTranscodeProcess;

        public MediaModule(
            IMvxLog logger,
            IFFMpegService fFMpegService,
            ITelemetryService telemetryService,
            string baseRoute)
            : base(baseRoute)
        {
            _logger = logger;
            _ffmpegService = fFMpegService;
            _telemetryService = telemetryService;
            _tokenSource = new CancellationTokenSource();
        }

        public override bool IsFinalHandler => false;

        protected override async Task OnRequestAsync(IHttpContext context)
        {
            var query = context.GetRequestQueryData();
            if (query.Count == 0 || !query.AllKeys.All(q => AppWebServer.AllowedQueryParameters.Contains(q)))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.StatusDescription = "You need to provide all the query params";
                context.SetHandled();
                _logger.Warn($"{nameof(OnRequestAsync)}: Query params does not contain the all the allowed values.");
                return;
            }
            try
            {
                string filepath = query[AppWebServer.FileQueryParameter];
                double seconds = double.Parse(query[AppWebServer.SecondsQueryParameter]);
                int videoStreamIndex = int.Parse(query[AppWebServer.VideoStreamIndexParameter]);
                int audioStreamIndex = int.Parse(query[AppWebServer.AudioStreamIndexParameter]);
                bool videoNeedsTranscode = bool.Parse(query[AppWebServer.VideoNeedsTranscode]);
                bool audioNeedsTranscode = bool.Parse(query[AppWebServer.AudioNeedsTranscode]);
                var hwAccelType = Enum.Parse<HwAccelDeviceType>(query[AppWebServer.HwAccelTypeToUse], true);
                string videoWidthAndHeight = query[AppWebServer.VideoWidthAndHeight];

                bool isVideoFile = FileUtils.IsVideoFile(filepath);
                bool isMusicFile = FileUtils.IsMusicFile(filepath);
                bool isHls = FileUtils.IsHls(filepath);

                if (!isVideoFile && !isMusicFile && !isHls)
                {
                    _logger.Warn($"{nameof(OnRequestAsync)}: File = {filepath} is not a video nor music file.");
                    return;
                }

                _logger.Info(
                    $"{nameof(OnRequestAsync)}: Handling request for file = {filepath} " +
                    $"with seconds = {seconds}, with videoStreamIndex = {videoStreamIndex}, " +
                    $"with audioStreamIndex = {audioStreamIndex}, " +
                    $"with transVideo = {videoNeedsTranscode}, " +
                    $"with transAudio = {audioNeedsTranscode}, " +
                    $"with hwAccelType = {hwAccelType}");

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
                    await _ffmpegService.TranscodeVideo(
                        context.Response.OutputStream,
                        filepath,
                        videoStreamIndex,
                        audioStreamIndex,
                        seconds,
                        videoNeedsTranscode,
                        audioNeedsTranscode,
                        hwAccelType,
                        _tokenSource.Token,
                        videoWidthAndHeight).ConfigureAwait(false);
                }
                else
                {
                    await using var memoryStream = await _ffmpegService.TranscodeMusic(
                        filepath,
                        audioStreamIndex,
                        seconds,
                        _tokenSource.Token).ConfigureAwait(false);
                    //TODO: THIS LENGTH IS NOT WORKING PROPERLY
                    context.Response.ContentLength64 = memoryStream.Length;
                    await memoryStream.CopyToAsync(context.Response.OutputStream, _tokenSource.Token)
                        .ConfigureAwait(false);
                }
                _logger.Info($"{nameof(OnRequestAsync)}: Request was successfully handled for file = {filepath}.");
            }
            catch (Exception e)
            {
                if (e is IOException || e is TaskCanceledException)
                    return;
                _logger.Error(e, $"{nameof(OnRequestAsync)}: Unknown error occurred");
                _telemetryService.TrackError(e);
            }
            finally
            {
                context.SetHandled();
            }
        }
    }
}
