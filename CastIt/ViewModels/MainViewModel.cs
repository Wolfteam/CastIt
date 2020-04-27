using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace CastIt.ViewModels
{
    public class MainViewModel : MvxViewModel
    {
        private bool _isPlaylistVisible;
        public bool IsPlaylistVisible
        {
            get => _isPlaylistVisible;
            set => SetProperty(ref _isPlaylistVisible, value);
        }

        public MvxObservableCollection<FileItemViewModel> Files { get; set; }
            = new MvxObservableCollection<FileItemViewModel>();

        public IMvxCommand TogglePlaylistVisibilityCommand { get; private set; }

        public MainViewModel()
        {
            TogglePlaylistVisibilityCommand = new MvxCommand(() =>
            {
                IsPlaylistVisible = !IsPlaylistVisible;
            });

            Files = new MvxObservableCollection<FileItemViewModel>
            {
                new FileItemViewModel
                {
                    Index = 1,
                    Filename = "B Gata H Kei  Nonononon.mp3",
                    Duration = 24,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\B Gata H Kei  Nonononon.mp3",
                    Size = "3.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 2,
                    Filename = "Nanahira  課金厨のうた -More Charin Ver.-.mp3",
                    Duration = 3,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\Nanahira  課金厨のうた -More Charin Ver.-.mp3",
                    Size = "8.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 3,
                    Filename = "Minami-Ke  Girl Jundo UP.mp3",
                    Duration = 4,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\Minami-Ke  Girl Jundo UP.mp3",
                    Size = "4.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 4,
                    Filename = "Hatsune Miku  Ai Kotoba.mp3",
                    Duration = 24,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\Hatsune Miku  Ai Kotoba.mp3",
                    Size = "3.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 5,
                    Filename = "Hatsune Miku  Konbini.mp3",
                    Duration = 3,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\Hatsune Miku  Konbini.mp3",
                    Size = "8.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 6,
                    Filename = "K-ON!  Fuwa Fuwa Time.mp3",
                    Duration = 4,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\K-ON!  Fuwa Fuwa Time.mp3",
                    Size = "4.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 7,
                    Filename = "K-ON!  Watashi no Koi wa Hotch Kiss.mp3",
                    Duration = 24,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\K-ON!  Watashi no Koi wa Hotch Kiss.mp3",
                    Size = "3.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 8,
                    Filename = "Nanahira  Hito to Usagi to Labyrinth.mp3",
                    Duration = 3,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\Nanahira  Hito to Usagi to Labyrinth.mp3",
                    Size = "8.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 9,
                    Filename = "S3RL  MTC.mp3",
                    Duration = 4,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\S3RL  MTC.mp3",
                    Size = "4.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 10,
                    Filename = "S3RL  R4V3 B0Y.mp3",
                    Duration = 24,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\S3RL  R4V3 B0Y.mp3",
                    Size = "3.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 11,
                    Filename = "S3RL  Waifu.mp3",
                    Duration = 3,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\S3RL  Waifu.mp3",
                    Size = "8.5 mb"
                },
                new FileItemViewModel
                {
                    Index = 12,
                    Filename = "S3RL  Happy Hardcore Tonight.mp3",
                    Duration = 4,
                    Extension = ".mp3",
                    Path = "C:\\Users\\Efrain Bastidas\\Music\\S3RL  Happy Hardcore Tonight.mp3",
                    Size = "4.5 mb"
                },
            };
        }
    }
}
