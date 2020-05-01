using CastIt.Interfaces;
using CastIt.Models.Messages;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
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
            set => SetProperty(ref _playedPercentage, value);
        }

        public string Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
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

        public bool Exists
            => System.IO.File.Exists(Path);
        public string Filename
            => System.IO.Path.GetFileName(Path);
        public string Size
            => GetFileSize();
        public string Extension
            => System.IO.Path.GetExtension(Path).ToUpper().Replace(".", "");
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
            IMvxNavigationService navigationService,
            ICastService castService)
            : base(textProvider, messenger, logger.GetLogFor<FileItemViewModel>(), navigationService)
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

        private string GetFileSize()
        {
            var fileInfo = new System.IO.FileInfo(Path);
            if (!fileInfo.Exists)
            {
                return "N/A";
            }

            var sizeInBytes = fileInfo.Length;
            float sizeInMb = sizeInBytes / 1024F / 1024F;
            return Math.Round(sizeInMb, 2) + " MB";
        }

        public async Task SetDuration()
        {
            if (!Exists)
            {
                Duration = GetText("Missing");
                return;
            }
            var seconds = await _castService.GetDuration(Path, true);
            if (seconds <= 0)
            {
                Duration = "N/A";
                return;
            }

            var time = TimeSpan.FromSeconds(seconds);
            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            if (time.Hours > 0)
                Duration = time.ToString(@"hh\:mm\:ss");
            else
                Duration = time.ToString(@"mm\:ss");
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
