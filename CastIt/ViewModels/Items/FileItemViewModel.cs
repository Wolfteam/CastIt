using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models.FFMpeg;
using CastIt.Models.Messages;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Items
{
    public class FileItemViewModel : BaseViewModel
    {
        #region Members
        private readonly ICastService _castService;
        private readonly IAppSettingsService _settingsService;
        private readonly IFFMpegService _ffmpegService;

        private bool _isSelected;
        private bool _isSeparatorTopLineVisible;
        private bool _isSeparatorBottomLineVisible;
        private string _duration;
        private int _position;
        private string _path;
        private double _playedPercentage;
        private bool _isBeingPlayed;
        private bool _loop;
        #endregion

        #region Properties
        public long Id { get; set; }
        public long PlayListId { get; set; }
        public double TotalSeconds { get; private set; }
        public bool PositionChanged { get; set; }

        public int Position
        {
            get => _position;
            set => this.RaiseAndSetIfChanged(ref _position, value);
        }

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public double PlayedPercentage
        {
            get => _playedPercentage;
            set => this.RaiseAndSetIfChanged(ref _playedPercentage, value);
        }

        public bool IsBeingPlayed
        {
            get => _isBeingPlayed;
            set => SetProperty(ref _isBeingPlayed, value);
        }

        public bool CanStartPlayingFromCurrentPercentage
            => PlayedPercentage > 0 && PlayedPercentage < 100;

        public bool WasPlayed
            => PlayedPercentage > 0 && PlayedPercentage <= 100;

        public string Duration
        {
            get => _duration;
            private set => SetProperty(ref _duration, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsSeparatorTopLineVisible
        {
            get => _isSeparatorTopLineVisible;
            set => SetProperty(ref _isSeparatorTopLineVisible, value);
        }

        public bool IsSeparatorBottomLineVisible
        {
            get => _isSeparatorBottomLineVisible;
            set => SetProperty(ref _isSeparatorBottomLineVisible, value);
        }

        public bool Loop
        {
            get => _loop;
            set => SetProperty(ref _loop, value);
        }

        public bool ShowFileDetails
            => _settingsService.ShowFileDetails;
        public bool IsLocalFile
            => FileUtils.IsLocalFile(Path);
        public bool IsUrlFile
            => FileUtils.IsUrlFile(Path);
        public bool Exists
            => IsLocalFile || IsUrlFile;
        public string Filename
            => _castService.GetFileName(Path);
        public string Size
            => _castService.GetFileSizeString(Path);
        public string Extension
            => _castService.GetExtension(Path);
        public string SubTitle
            => $"{Extension}, {Size}";

        public FFProbeFileInfo FileInfo { get; set; }
        #endregion

        #region Commands
        public IMvxCommand PlayCommand { get; private set; }
        public IMvxCommand PlayFromTheBeginingCommand { get; private set; }
        public IMvxCommand OpenFileLocationCommand { get; private set; }
        public IMvxCommand ToggleLoopCommand { get; private set; }
        #endregion

        public FileItemViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            ICastService castService,
            IAppSettingsService settingsService,
            IFFMpegService ffmpegService)
            : base(textProvider, messenger, logger.GetLogFor<FileItemViewModel>())
        {
            _castService = castService;
            _settingsService = settingsService;
            _ffmpegService = ffmpegService;
        }

        public override void SetCommands()
        {
            base.SetCommands();

            PlayCommand = new MvxCommand(() => Messenger.Publish(new PlayFileMessage(this)));

            PlayFromTheBeginingCommand = new MvxCommand(() => Messenger.Publish(new PlayFileMessage(this, true)));

            OpenFileLocationCommand = new MvxCommand(() =>
            {
                var psi = new ProcessStartInfo("explorer.exe", "/n /e,/select," + Path);
                Process.Start(psi);
            });

            ToggleLoopCommand = new MvxCommand(() =>
            {
                Loop = !Loop;
                Messenger.Publish(new LoopFileMessage(this));
            });
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            SubscriptionTokens.Add(Messenger.Subscribe<ShowFileDetailsMessage>(_ => RaisePropertyChanged(() => ShowFileDetails)));
        }

        public void ShowItemSeparators(bool showTop, bool showBottom)
        {
            IsSeparatorBottomLineVisible = showBottom;
            IsSeparatorTopLineVisible = showTop;
        }

        public void HideItemSeparators()
        {
            IsSeparatorBottomLineVisible
                = IsSeparatorTopLineVisible = false;
        }

        public async Task SetFileInfo(CancellationToken token)
        {
            if (IsUrlFile)
            {
                FileInfo = new FFProbeFileInfo
                {
                    Format = new FileInfoFormat()
                };
                SetDuration(-1);
                return;
            }

            FileInfo = await _ffmpegService.GetFileInfo(Path, token);

            SetDuration(FileInfo?.Format?.Duration ?? -1);
        }

        public void SetDuration(double seconds)
        {
            if (!Exists)
            {
                Duration = GetText("Missing");
                return;
            }
            TotalSeconds = seconds;
            if (seconds <= 0)
            {
                Duration = "N/A";
                return;
            }
            var time = TimeSpan.FromSeconds(seconds);
            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            if (time.Hours > 0)
                Duration = time.ToString(AppConstants.FullElapsedTimeFormat);
            else
                Duration = time.ToString(AppConstants.ShortElapsedTimeFormat);
        }

        public void ListenEvents()
        {
            CleanUp();
            _castService.OnPositionChanged += OnPositionChanged;
            _castService.OnEndReached += OnEndReached;
            IsBeingPlayed = true;
        }

        public void CleanUp()
        {
            _castService.OnPositionChanged -= OnPositionChanged;
            _castService.OnEndReached -= OnEndReached;
            IsBeingPlayed = false;
        }

        private void OnPositionChanged(double position)
            => PlayedPercentage = position;

        private void OnEndReached()
            => OnPositionChanged(100);
    }
}
