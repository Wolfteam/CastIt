﻿using CastIt.Application.Common;
using CastIt.Application.Common.Extensions;
using CastIt.Application.Interfaces;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.Infrastructure.Interfaces;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using CastIt.Server.Interfaces;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Items
{
    public class FileItemViewModel : BaseViewModel
    {
        #region Members
        private readonly ICastService _castService;
        private readonly IAppSettingsService _settingsService;
        private readonly IFFmpegService _ffmpegService;
        private readonly IAppWebServer _appWebServer;
        private readonly IAppDataService _playListsService;
        private readonly IFileService _fileService;

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

        public bool CanStartPlayingFromCurrentPercentage
            => PlayedPercentage > 0 && PlayedPercentage < 100;

        public bool WasPlayed
            => PlayedPercentage > 0 && PlayedPercentage <= 100;

        public string Duration
        {
            get => _duration;
            private set => this.RaiseAndSetIfChanged(ref _duration, value);
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
                SetProperty(ref _loop, value);
                _appWebServer.OnFileChanged?.Invoke(PlayListId);
            }
        }

        public bool ShowFileDetails
            => _settingsService.ShowFileDetails;
        public bool IsLocalFile
            => _fileService.IsLocalFile(Path);
        public bool IsUrlFile
            => _fileService.IsUrlFile(Path);
        public bool Exists
            => IsLocalFile || IsUrlFile;

        public string Filename
        {
            get => _fileName ??= IsCached
                ? Name
                : _fileService.IsLocalFile(Path)
                    ? _fileService.GetFileName(Path)
                    : !string.IsNullOrEmpty(Name)
                        ? Name
                        : Path;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        public string Size
            => _fileService.GetFileSizeString(Path);
        public string Extension
            => _fileService.GetExtension(Path);
        public string Resolution
            => !IsLocalFile
                ? string.Empty
                : FileInfo?.GetVideoResolution();
        public string SubTitle
            => IsCached ? Description : Extension.AppendDelimiter("|", Size, Resolution);

        public bool IsCached
            => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Description) && !string.IsNullOrWhiteSpace(Path);

        public FFProbeFileInfo FileInfo { get; set; }

        public double PlayedSeconds
            => PlayedPercentage * TotalSeconds / 100;

        //had to do it this way, so the ui does not call this prop each time i scroll
        public string PlayedTime
        {
            get => _playedTime ??= FileFormatConstants.FormatDuration(PlayedSeconds);
            set => this.RaiseAndSetIfChanged(ref _playedTime, value);
        }
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
            ICastService castService,
            IAppSettingsService settingsService,
            IFFmpegService ffmpegService,
            IAppWebServer appWebServer,
            IAppDataService playListsService,
            IFileService fileService)
            : base(textProvider, messenger, logger)
        {
            _castService = castService;
            _settingsService = settingsService;
            _ffmpegService = ffmpegService;
            _appWebServer = appWebServer;
            _playListsService = playListsService;
            _fileService = fileService;
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

        public async Task SetFileInfo(CancellationToken token, bool force = true)
        {
            if (IsUrlFile)
            {
                FileInfo = new FFProbeFileInfo
                {
                    Format = new FileInfoFormat()
                };
                await SetDuration(TotalSeconds > 0 ? TotalSeconds : -1);
                return;
            }

            if (IsCached && !force)
            {
                var maxDate = DateTime.Now.AddDays(-3);
                bool update = Exists && File.GetLastAccessTime(Path) < maxDate && UpdatedAt <= maxDate;
                await SetDuration(TotalSeconds, update);
                return;
            }

            var fileInfo = await _ffmpegService.GetFileInfo(Path, token);
            if (fileInfo != null)
            {
                FileInfo = fileInfo;
            }

            var duration = FileInfo?.Format?.Duration ?? -1;
            await SetDuration(duration);
            await RaisePropertyChanged(nameof(SubTitle));
        }

        public async Task SetDuration(double seconds, bool update = true)
        {
            await RaisePropertyChanged(() => IsLocalFile);
            await RaisePropertyChanged(() => IsUrlFile);
            await RaisePropertyChanged(() => Exists);
            if (!Exists)
            {
                TotalSeconds = 0;
                Duration = GetText("Missing");
                return;
            }
            TotalSeconds = seconds;

            if (update)
                await _playListsService.UpdateFile(Id, Filename, SubTitle, seconds);

            if (seconds <= 0)
            {
                Duration = "N/A";
                return;
            }
            Duration = FileFormatConstants.FormatDuration(seconds);
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
        {
            PlayedPercentage = position;
            PlayedTime = FileFormatConstants.FormatDuration(PlayedSeconds);
        }

        private void OnEndReached()
            => OnPositionChanged(100);

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
    }
}
