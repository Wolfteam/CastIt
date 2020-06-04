using CastIt.Common.Utils;
using CastIt.Interfaces;
using EmbedIO;
using MvvmCross.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Server
{
    internal class VideoModule : WebModuleBase
    {
        private readonly IMvxLog _logger;
        private readonly IFFMpegService _ffmpegService;

        private CancellationTokenSource _tokenSource;
        private bool _checkTranscodeProcess;

        public VideoModule(
            IMvxLog logger,
            IFFMpegService fFMpegService,
            string baseRoute)
            : base(baseRoute)
        {
            _logger = logger;
            _ffmpegService = fFMpegService;

            _tokenSource = new CancellationTokenSource();
        }

        public override bool IsFinalHandler => false;

        protected override Task OnRequestAsync(IHttpContext context)
        {
            var path = context.RequestedPath;
            var verb = context.Request.HttpVerb;
            var query = context.GetRequestQueryData();
            if (query.Count == 0 || !query.AllKeys.All(q => AppWebServer.AllowedQueryParameters.Contains(q)))
            {
                context.SetHandled();
                _logger.Warn($"{nameof(OnRequestAsync)}: Query params does not contain the all the allowed values.");
                return Task.CompletedTask;
            }
            try
            {
                string filepath = query[AppWebServer.FileQueryParameter];
                double seconds = double.Parse(query[AppWebServer.SecondsQueryParameter]);
                int videoStreamIndex = int.Parse(query[AppWebServer.VideoStreamIndexParameter]);
                int audioStreamIndex = int.Parse(query[AppWebServer.AudioStreamIndexParameter]);

                if (!File.Exists(filepath))
                {
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    _logger.Warn($"{nameof(OnRequestAsync)}: File = {filepath} does not exist.");
                    return Task.CompletedTask;
                }

                _logger.Info(
                    $"{nameof(OnRequestAsync)}: Handling request for file = {filepath} " +
                    $"with seconds = {seconds}, with videoStreamIndex = {videoStreamIndex} " +
                    $"and audioStreamIndex = {audioStreamIndex}");

                context.Response.ContentType = _ffmpegService.GetOutputTranscodeMimeType(filepath);

                context.Response.DisableCaching();

                if (_checkTranscodeProcess)
                {
                    _tokenSource.Cancel();
                    _tokenSource = new CancellationTokenSource();
                    _ffmpegService.KillTranscodeProcess();
                }

                _checkTranscodeProcess = true;
                if (FileUtils.IsVideoFile(filepath))
                {
                    return _ffmpegService.TranscodeVideo(
                        context.Response.OutputStream,
                        filepath,
                        videoStreamIndex,
                        audioStreamIndex,
                        seconds,
                        _tokenSource.Token);
                }
                return _ffmpegService.TranscodeMusic(context, filepath, audioStreamIndex, seconds, _tokenSource.Token);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{nameof(OnRequestAsync)}: Unknown error occcured");
            }
            finally
            {
                context.SetHandled();
            }
            return Task.CompletedTask;
        }
    }
}
