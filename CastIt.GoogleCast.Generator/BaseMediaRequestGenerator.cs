using CastIt.Domain.Enums;
using CastIt.FFmpeg;
using CastIt.GoogleCast.Models.Media;
using CastIt.GoogleCast.Models.Play;
using CastIt.GoogleCast.Shared.Enums;
using CastIt.Server.Shared;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Generator
{
    public abstract class BaseMediaRequestGenerator
    {
        public const int SubTitleDefaultTrackId = 1;

        protected readonly ILogger Logger;
        protected readonly IFFmpegService FFmpeg;
        protected readonly IBaseServerService Server;
        protected readonly IFileService FileService;

        protected BaseMediaRequestGenerator(
            ILogger logger,
            IFFmpegService fFmpeg,
            IBaseServerService server,
            IFileService fileService)
        {
            Logger = logger;
            FFmpeg = fFmpeg;
            Server = server;
            FileService = fileService;
        }

        protected void SetStreams(ServerFileItem file, bool fileOptionsChanged)
        {
            if (fileOptionsChanged)
                return;
            Logger.LogInformation($"{nameof(SetStreams)}: Setting the video and audio streams...");
            file
                .CleanAllStreams()
                .SetVideoStreams()
                .SetAudioStreams();
        }

        protected async Task SetSubtitlesIfAny(
            ServerFileItem file,
            ServerAppSettings settings,
            PlayMediaRequest request,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation($"{nameof(SetSubtitlesIfAny)}: Setting subtitles if needed...");
            bool useSubTitleStream = request.SubtitleStreamIndex >= 0;
            string selectedSubtitlePath = file.CurrentFileSubTitles.Find(f => f.IsSelected)?.Path;
            if (!useSubTitleStream && string.IsNullOrEmpty(selectedSubtitlePath))
            {
                Logger.LogInformation($"{nameof(SetSubtitlesIfAny)}: No subtitles were specified");
                return;
            }

            Logger.LogInformation($"{nameof(SetSubtitlesIfAny)}: Subtitles were specified, generating a compatible one...");
            string subtitleLocation = useSubTitleStream ? file.Path : selectedSubtitlePath;
            string subTitleFilePath = FileService.GetSubTitleFilePath();
            await FFmpeg.GenerateSubTitles(
                subtitleLocation,
                subTitleFilePath,
                request.SeekSeconds,
                useSubTitleStream ? request.SubtitleStreamIndex : 0,
                settings.SubtitleDelayInSeconds,
                cancellationToken);

            var subtitle = new Track
            {
                TrackId = SubTitleDefaultTrackId,
                SubType = TextTrackType.Subtitles,
                Type = TrackType.Text,
                Name = "English",
                Language = "en-US",
                TrackContentId = Server.GetSubTitleUrl()
            };

            request.MediaInfo.Tracks.RemoveAll(t => t.TrackId == SubTitleDefaultTrackId);
            request.ActiveTrackIds.RemoveAll(t => t == SubTitleDefaultTrackId);

            request.MediaInfo.Tracks.Add(subtitle);
            request.MediaInfo.TextTrackStyle = new TextTrackStyle
            {
                ForegroundColor = settings.CurrentSubtitleFgColor == SubtitleFgColorType.White
                    ? Color.WhiteSmoke
                    : Color.Yellow,
                BackgroundColor = Color.Transparent,
                EdgeColor = Color.Black,
                FontScale = (int)settings.CurrentSubtitleFontScale / 100,
                WindowType = TextTrackWindowType.Normal,
                EdgeType = TextTrackEdgeType.Raised,
                FontStyle = settings.CurrentSubtitleFontStyle,
                FontGenericFamily = settings.CurrentSubtitleFontFamily
            };
            request.ActiveTrackIds.Add(SubTitleDefaultTrackId);
            Logger.LogInformation($"{nameof(SetSubtitlesIfAny)}: Subtitles were generated");
        }
    }
}
