using CastIt.Common;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Items
{
    public class FileItemViewModel : BaseViewModel
    {
        #region Members
        private readonly ICastService _castService;
        private bool _isSelected;
        private bool _isSeparatorTopLineVisible;
        private bool _isSeparatorBottomLineVisible;
        private string _duration;
        private int _position;
        private string _path;
        private double _playedPercentage;
        #endregion

        #region Properties
        public long Id { get; set; }
        public long PlayListId { get; set; }
        public long TotalSeconds { get; private set; }

        public int Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public double PlayedPercentage
        {
            get => _playedPercentage;
            set
            {
                if (value == _playedPercentage)
                    return;
                SetProperty(ref _playedPercentage, value);
            }
        }

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

        public bool IsLocalFile 
            => _castService.IsLocalFile(Path);
        public bool IsUrlFile
            => _castService.IsUrlFile(Path);
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
        #endregion

        #region Commands
        public IMvxCommand PlayCommand { get; private set; }
        public IMvxCommand PlayFromTheBeginingCommand { get; private set; }
        public IMvxCommand OpenFileLocationCommand { get; private set; }
        #endregion

        public FileItemViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            ICastService castService)
            : base(textProvider, messenger, logger.GetLogFor<FileItemViewModel>())
        {
            _castService = castService;
        }

        public override void SetCommands()
        {
            base.SetCommands();

            PlayCommand = new MvxCommand(() =>
            {
                //TODO: DOUBLE CHECK THAT THIS FILE IS NOT BEING ALREADY PLAYED
                Messenger.Publish(new PlayFileMsg(this));
            });

            PlayFromTheBeginingCommand = new MvxCommand(() =>
            {
                _castService.GoToPosition(0);
            });

            OpenFileLocationCommand = new MvxCommand(() =>
            {
                var psi = new ProcessStartInfo("explorer.exe", "/n /e,/select," + Path);
                Process.Start(psi);
            });
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

        public async Task SetDuration()
        {
            if (!Exists)
            {
                Duration = GetText("Missing");
                return;
            }
            //TODO: IF THE USER OPENS THE APP, AND HAS A LOT OF ITEMS, AND THEN, IT CLOSES THE APP, A CRASH MAY BE THROWN
            var seconds = await _castService.GetDuration(Path);
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
        }

        public void CleanUp()
        {
            _castService.OnPositionChanged -= OnPositionChanged;
            _castService.OnEndReached -= OnEndReached;
        }

        private void OnPositionChanged(float position)
            => PlayedPercentage = position;

        private void OnEndReached()
            => OnPositionChanged(100);
    }
}
