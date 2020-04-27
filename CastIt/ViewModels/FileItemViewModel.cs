using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace CastIt.ViewModels
{
    public class FileItemViewModel : MvxViewModel
    {
        private bool _isSelected;
        private bool _isSeparatorTopLineVisible;
        private bool _isSeparatorBottomLineVisible;

        public int Index { get; set; }
        public string Filename { get; set; }
        public string Size { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public long Duration { get; set; }
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

        public IMvxCommand PlayFileCommand { get; private set; }

        public FileItemViewModel()
        {
            PlayFileCommand = new MvxCommand(() =>
            {
                System.Diagnostics.Debug.WriteLine("Double click");
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
    }
}
