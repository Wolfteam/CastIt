using CastIt.Common.Utils;
using CastIt.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using System;
using System.Threading.Tasks;
using Xabe.FFmpeg.Downloader;

namespace CastIt.ViewModels.Dialogs
{
    public class DownloadDialogViewModel : BaseDialogViewModelResult<bool>
    {
        #region Members
        private readonly IMvxNavigationService _navigationService;
        private readonly ITelemetryService _telemetryService;

        private bool _isDownloading;
        #endregion

        #region Properties
        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }
        #endregion

        public DownloadDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            IMvxNavigationService navigationService,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.GetLogFor<DownloadDialogViewModel>())
        {
            _navigationService = navigationService;
            _telemetryService = telemetryService;
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
                var path = FileUtils.GetFFMpegFolder();
                Logger.Info($"{nameof(DownloadMissingFiles)}: Downloading missing files. Save path is = {path}");
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, path);
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
