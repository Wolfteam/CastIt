using CastIt.Application.Interfaces;
using CastIt.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace CastIt.ViewModels.Dialogs
{
    public class DownloadDialogViewModel : BaseDialogViewModelResult<bool>
    {
        #region Members
        private readonly IMvxNavigationService _navigationService;
        private readonly ITelemetryService _telemetryService;
        private readonly IFileService _fileService;

        private bool _isDownloading;
        private double _downloadedProgress;
        private string _downloadedProgressText;
        #endregion

        #region Properties
        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        public double DownloadedProgress
        {
            get => _downloadedProgress;
            set => this.RaiseAndSetIfChanged(ref _downloadedProgress, value);
        }

        public string DownloadedProgressText
        {
            get => _downloadedProgressText;
            set => this.RaiseAndSetIfChanged(ref _downloadedProgressText, value);
        }
        #endregion

        public DownloadDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            IMvxNavigationService navigationService,
            ITelemetryService telemetryService,
            IFileService fileService)
            : base(textProvider, messenger, logger.GetLogFor<DownloadDialogViewModel>())
        {
            _navigationService = navigationService;
            _telemetryService = telemetryService;
            _fileService = fileService;
        }

        public override void Prepare()
        {
            base.Prepare();
            Title = GetText("DownloadMissingFiles");
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(DownloadMissingFiles);

            CloseCommand = new MvxAsyncCommand(async () => await _navigationService.Close(this, false));
        }

        private async Task DownloadMissingFiles()
        {
            bool filesWereDownloaded = false;
            IsDownloading = true;
            try
            {
                var path = _fileService.GetFFmpegFolder();
                Logger.Info($"{nameof(DownloadMissingFiles)}: Downloading missing files. Save path is = {path}");
                var progress = new Progress<ProgressInfo>((p) =>
                {
                    var downloaded = (double)p.DownloadedBytes / p.TotalBytes * 100;
                    if (downloaded > 100)
                        downloaded = 100;
                    if (downloaded > DownloadedProgress)
                    {
                        DownloadedProgressText = $"{_fileService.GetBytesReadable(p.DownloadedBytes)} / {_fileService.GetBytesReadable(p.TotalBytes)}";
                        DownloadedProgress = downloaded;
                    }
                });
                await Task.Delay(500);
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, path, progress).ConfigureAwait(false);
                filesWereDownloaded = true;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"{nameof(DownloadMissingFiles)}: An unknown error occurred");
                _telemetryService.TrackError(e);
            }
            IsDownloading = false;

            await _navigationService.Close(this, filesWereDownloaded);
        }
    }
}
