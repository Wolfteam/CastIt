using AutoMapper;
using CastIt.Domain;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using Microsoft.Extensions.Logging;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Diagnostics;

namespace CastIt.ViewModels.Items
{
    public class FileItemViewModel : BaseViewModel
    {
        #region Members
        private readonly IDesktopAppSettingsService _settingsService;
        private readonly ICastItHubClientService _castItHub;

        private bool _isSeparatorTopLineVisible;
        private bool _isSeparatorBottomLineVisible;
        private string _duration;
        private int _position;
        private string _path;
        private double _playedPercentage;
        private bool _isBeingPlayed;
        private bool _loop;
        private string _playedTime;
        private string _fileName;
        #endregion

        #region Properties
        public long Id { get; set; }
        public long PlayListId { get; set; }
        public double TotalSeconds { get; set; }
        public bool PositionChanged { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedAt { get; set; }

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

        public bool CanStartPlayingFromCurrentPercentage { get; set; }

        public bool WasPlayed { get; set; }

        public string Duration
        {
            get => _duration;
            set
            {
                var updated = value.Replace(AppWebServerConstants.MissingFileText, GetText("Missing"));
                this.RaiseAndSetIfChanged(ref _duration, updated);
            }
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
            set
            {
                bool triggerChange = _loop != value;
                SetProperty(ref _loop, value);
                if (triggerChange && !Loading)
                    LoopFile();
            }
        }

        public bool ShowFileDetails
            => _settingsService.ShowFileDetails;
        public bool IsLocalFile { get; set; }
        public bool IsUrlFile { get; set; }
        public bool Exists { get; set; }

        public string Filename
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        public string Size { get; set; }
        public string Extension { get; set; }
        public string Resolution { get; set; }
        public string SubTitle { get; set; }
        public bool IsCached { get; set; }

        public double PlayedSeconds { get; set; }

        //had to do it this way, so the ui does not call this prop each time i scroll
        public string PlayedTime
        {
            get => _playedTime;
            set => this.RaiseAndSetIfChanged(ref _playedTime, value);
        }

        public AppFileType Type { get; set; }

        public bool Loading { get; private set; }
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
            ILogger<FileItemViewModel> logger,
            IDesktopAppSettingsService settingsService,
            ICastItHubClientService castItHub)
            : base(textProvider, messenger, logger)
        {
            _settingsService = settingsService;
            _castItHub = castItHub;
        }

        public static FileItemViewModel From(FileItemResponseDto file, IMapper mapper)
        {
            var vm = Mvx.IoCProvider.Resolve<FileItemViewModel>();
            vm.Loading = true;
            mapper.Map(file, vm);
            vm.Loading = false;
            return vm;
        }

        public override void SetCommands()
        {
            base.SetCommands();

            PlayCommand = new MvxCommand(() => Messenger.Publish(new PlayFileMessage(this)));

            PlayFromTheBeginingCommand = new MvxCommand(() => Messenger.Publish(new PlayFileMessage(this, true)));

            OpenFileLocationCommand = new MvxCommand(OpenFileLocation);

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

        public void OnChange(FileItemResponseDto file)
        {
            IsBeingPlayed = file.IsBeingPlayed;
            Position = file.Position;
            Duration = file.Duration;
            PlayedSeconds = file.PlayedSeconds;
            PlayedPercentage = file.PlayedPercentage;
            Loop = file.Loop;
            PlayedTime = FileFormatConstants.FormatDuration(PlayedSeconds);
        }

        public void OnStopped()
        {
            IsBeingPlayed = false;
        }

        public void OnEndReached()
        {
            OnStopped();
            PlayedPercentage = 100;
        }

        private void OpenFileLocation()
        {
            if (IsLocalFile)
            {
                var psi = new ProcessStartInfo("explorer.exe", "/n /e,/select," + @$"""{Path}""");
                Process.Start(psi);
            }
            else if (IsUrlFile)
            {
                Process.Start(new ProcessStartInfo(Path)
                {
                    UseShellExecute = true
                });
            }
            else
            {
                Messenger.Publish(new SnackbarMessage(this, GetText("FileCouldntBeOpened")));
            }
        }

        private async void LoopFile()
        {
            await _castItHub.LoopFile(PlayListId, Id, Loop);
        }
    }
}
