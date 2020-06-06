using MvvmCross.ViewModels;

namespace CastIt.ViewModels.Items
{
    public class FileItemOptionsViewModel : MvxViewModel
    {
        private string _text;
        private bool _isSelected;
        private bool _isEnabled;

        public int Id { get; set; }
        public bool IsVideo { get; set; }
        public bool IsAudio { get; set; }
        public bool IsSubTitle { get; set; }
        public bool IsQuality { get; set; }
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }
}
