using CastIt.Common;
using CastIt.Common.Utils;
using CastIt.Domain.Enums;
using CastIt.GoogleCast.Shared.Enums;
using CastIt.Infrastructure.Models;
using CastIt.Interfaces;
using CastIt.Models;
using CastIt.Models.Messages;
using CastIt.ViewModels.Dialogs;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CastIt.ViewModels
{
    public class SettingsViewModel : BasePopupViewModel
    {
        #region Members
        private readonly IDesktopAppSettingsService _settingsService;
        private readonly IMvxNavigationService _navigationService;
        private readonly ICastItHubClientService _castItHub;

        private bool _updatingSettings;
        private bool _startFilesFromTheStart;
        private bool _playNextFileAutomatically;
        private bool _forceVideoTranscode;
        private bool _forceAudioTranscode;
        private Item _videoScale;
        private bool _enableHardwareAcceleration;
        private Item _currentSubtitleFgColor;
        private Item _currentSubtitleBgColor;
        private Item _currentSubtitleFontScale;
        private Item _currentSubtitleFontStyle;
        private Item _currentSubtitleFontFamily;
        private double _subtitleDelayInSeconds;
        private bool _loadFirstSubtitleFoundAutomatically;
        private string _fFmpegExePath;
        private string _ffprobeExePath;

        private readonly MvxInteraction<string> _changeSelectedAccentColor = new MvxInteraction<string>();
        #endregion

        #region Properties
        public MvxObservableCollection<Item> Themes
            => GetThemes();
        public Item CurrentTheme
        {
            get => Themes.First(l => l.Id == _settingsService.AppTheme.ToString());
            set
            {
                if (value == null)
                    return;
                var selectedTheme = (AppThemeType)Enum.Parse(typeof(AppThemeType), value.Id, true);
                WindowsUtils.ChangeTheme(selectedTheme, _settingsService.AccentColor);
                _settingsService.AppTheme = selectedTheme;
                RaisePropertyChanged(() => CurrentTheme);
            }
        }

        public MvxObservableCollection<Item> Languages
            => GetLanguages();
        public Item CurrentLanguage
        {
            get => Languages.First(l => l.Id == _settingsService.Language.ToString());
            set
            {
                if (value == null)
                    return;
                var selectedLang = (AppLanguageType)Enum.Parse(typeof(AppLanguageType), value.Id, true);
                _settingsService.Language = selectedLang;
                TextProvider.SetLanguage(selectedLang, true);
                RaisePropertyChanged(() => CurrentLanguage);
            }
        }

        public List<string> AccentColors
            => AppConstants.AppAccentColors.ToList();

        public string CurrentAccentColor
            => _settingsService.AccentColor;

        public bool ShowFileDetails
        {
            get => _settingsService.ShowFileDetails;
            set
            {
                _settingsService.ShowFileDetails = value;
                Messenger.Publish(new ShowFileDetailsMessage(this, value));
                RaisePropertyChanged(() => ShowFileDetails);
            }
        }

        public bool StartFilesFromTheStart
        {
            get => _startFilesFromTheStart;
            set
            {
                this.RaiseAndSetIfChanged(ref _startFilesFromTheStart, value);
                TriggerSettingsChanged();
            }
        }

        public bool PlayNextFileAutomatically
        {
            get => _playNextFileAutomatically;
            set
            {
                this.RaiseAndSetIfChanged(ref _playNextFileAutomatically, value);
                TriggerSettingsChanged();
            }
        }

        public bool ForceVideoTranscode
        {
            get => _forceVideoTranscode;
            set
            {
                this.RaiseAndSetIfChanged(ref _forceVideoTranscode, value);
                TriggerSettingsChanged();
            }
        }

        public bool ForceAudioTranscode
        {
            get => _forceAudioTranscode;
            set
            {
                this.RaiseAndSetIfChanged(ref _forceAudioTranscode, value);
                TriggerSettingsChanged();
            }
        }

        public MvxObservableCollection<Item> VideoScales { get; }

        public Item CurrentVideoScale
        {
            get => _videoScale;
            set
            {
                if (value == null)
                    return;
                this.RaiseAndSetIfChanged(ref _videoScale, value);
                TriggerSettingsChanged();
            }
        }

        public bool EnableHardwareAcceleration
        {
            get => _enableHardwareAcceleration;
            set
            {
                this.RaiseAndSetIfChanged(ref _enableHardwareAcceleration, value);
                TriggerSettingsChanged();
            }
        }

        public bool MinimizeToTray
        {
            get => _settingsService.MinimizeToTray;
            set
            {
                _settingsService.MinimizeToTray = value;
                RaisePropertyChanged(() => MinimizeToTray);
            }
        }

        public bool ShowPlayListTotalDuration
        {
            get => _settingsService.ShowPlayListTotalDuration;
            set
            {
                _settingsService.ShowPlayListTotalDuration = value;
                Messenger.Publish(new ShowPlayListTotalDurationMessage(this, value));
                RaisePropertyChanged(() => ShowPlayListTotalDuration);
            }
        }

        public bool UseGridViewForPlayLists
        {
            get => _settingsService.UseGridViewForPlayLists;
            set
            {
                _settingsService.UseGridViewForPlayLists = value;
                Messenger.Publish(new UseGridViewMessage(this, value));
                RaisePropertyChanged(() => UseGridViewForPlayLists);
            }
        }

        #region Subtitles
        public MvxObservableCollection<Item> SubtitleFgColors { get; }
        public MvxObservableCollection<Item> SubtitleBgColors { get; }
        public MvxObservableCollection<Item> SubtitleFontStyles { get; }
        public MvxObservableCollection<Item> SubtitleFontFamilies { get; }
        public MvxObservableCollection<Item> SubtitleFontScales { get; }

        public Item CurrentSubtitleFgColor
        {
            get => _currentSubtitleFgColor;
            set
            {
                if (value == null)
                    return;
                this.RaiseAndSetIfChanged(ref _currentSubtitleFgColor, value);
                TriggerSettingsChanged();
            }
        }

        public Item CurrentSubtitleBgColor
        {
            get => _currentSubtitleBgColor;
            set
            {
                if (value == null)
                    return;
                this.RaiseAndSetIfChanged(ref _currentSubtitleBgColor, value);
                TriggerSettingsChanged();
            }
        }

        public Item CurrentSubtitleFontScale
        {
            get => _currentSubtitleFontScale;
            set
            {
                if (value == null)
                    return;
                this.RaiseAndSetIfChanged(ref _currentSubtitleFontScale, value);
                TriggerSettingsChanged();
            }
        }

        public Item CurrentSubtitleFontStyle
        {
            get => _currentSubtitleFontStyle;
            set
            {
                if (value == null)
                    return;
                this.RaiseAndSetIfChanged(ref _currentSubtitleFontStyle, value);
                TriggerSettingsChanged();
            }
        }

        public Item CurrentSubtitleFontFamily
        {
            get => _currentSubtitleFontFamily;
            set
            {
                if (value == null)
                    return;
                this.RaiseAndSetIfChanged(ref _currentSubtitleFontFamily, value);
                TriggerSettingsChanged();
            }
        }

        public string SubtitleDelayText
            => $"{GetText("SubtitleDelay")} ({GetText("XSeconds", $"{(SubtitleDelay == 0 ? 0 : SubtitleDelay)}")})";
        public double SubtitleDelay
        {
            get => _subtitleDelayInSeconds;
            set
            {
                this.RaiseAndSetIfChanged(ref _subtitleDelayInSeconds, Math.Round(value, 1));
                TriggerSettingsChanged();
                RaisePropertyChanged(() => SubtitleDelayText);
            }
        }

        public bool LoadFirstSubtitleFoundAutomatically
        {
            get => _loadFirstSubtitleFoundAutomatically;
            set
            {
                this.RaiseAndSetIfChanged(ref _loadFirstSubtitleFoundAutomatically, value);
                TriggerSettingsChanged();
            }
        }
        #endregion
        #endregion

        #region Commands
        public IMvxCommand<string> AccentColorChangedCommand { get; private set; }
        public IMvxAsyncCommand OpenAboutDialogCommand { get; private set; }
        #endregion

        #region Interactos
        public IMvxInteraction<string> ChangeSelectedAccentColor
            => _changeSelectedAccentColor;
        #endregion

        public SettingsViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<SettingsViewModel> logger,
            IDesktopAppSettingsService settingsService,
            IMvxNavigationService navigationService,
            ICastItHubClientService castItHub)
            : base(textProvider, messenger, logger)
        {
            _settingsService = settingsService;
            _navigationService = navigationService;
            _castItHub = castItHub;

            VideoScales = GetVideoScales();
            SubtitleBgColors = GetSubtitleBgColors();
            SubtitleFgColors = GetSubtitleFgColors();
            SubtitleFontScales = GetFontScales();
            SubtitleFontStyles = GetSubtitleFontStyles();
            SubtitleFontFamilies = GetSubtitleFontFamilies();

            _castItHub.OnPlayerSettingsChanged += OnSettingsChange;
        }

        public override void SetCommands()
        {
            base.SetCommands();
            AccentColorChangedCommand = new MvxCommand<string>((hexColor) =>
            {
                _settingsService.AccentColor = hexColor;
                _changeSelectedAccentColor.Raise(hexColor);
                WindowsUtils.ChangeTheme(_settingsService.AppTheme, _settingsService.AccentColor);
            });

            OpenAboutDialogCommand = new MvxAsyncCommand(
                async () => await _navigationService.Navigate<AboutDialogViewModel>());
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            SubscriptionTokens.AddRange(new []
            {
                Messenger.Subscribe<FfmpegPathChangedMessage>(msg =>
                {
                    _fFmpegExePath = Path.Combine(msg.FolderPath, "ffmpeg.exe");
                    _ffprobeExePath = Path.Combine(msg.FolderPath, "ffprobe.exe");
                    TriggerSettingsChanged();
                })
            });
        }

        public void CleanUp()
        {
            _castItHub.OnPlayerSettingsChanged -= OnSettingsChange;
        }

        private MvxObservableCollection<Item> GetThemes()
        {
            var themes = Enum.GetValues(typeof(AppThemeType)).Cast<AppThemeType>().Select(theme => new Item
            {
                Id = theme.ToString(),
                Text = GetText(theme.ToString())
            });

            return new MvxObservableCollection<Item>(themes);
        }

        private MvxObservableCollection<Item> GetLanguages()
        {
            var languages = Enum.GetValues(typeof(AppLanguageType)).Cast<AppLanguageType>().Select(lang => new Item
            {
                Id = lang.ToString(),
                Text = GetText(lang.ToString())
            });

            return new MvxObservableCollection<Item>(languages);
        }

        private MvxObservableCollection<Item> GetVideoScales()
        {
            var scales = new List<Item>
            {
                new Item
                {
                    Id = nameof(VideoScaleType.Original),
                    Text = GetText("Original")
                },

                new Item
                {
                    Id = nameof(VideoScaleType.Hd),
                    Text = "720p"
                },

                new Item
                {
                    Id = nameof(VideoScaleType.FullHd),
                    Text = "1080p"
                }
            };

            return new MvxObservableCollection<Item>(scales);
        }

        private MvxObservableCollection<Item> GetSubtitleFgColors()
        {
            var colors = Enum.GetValues(typeof(SubtitleFgColorType)).Cast<SubtitleFgColorType>().Select(color => new Item
            {
                Id = color.ToString(),
                Text = GetText(color.ToString())
            });

            return new MvxObservableCollection<Item>(colors);
        }

        private MvxObservableCollection<Item> GetSubtitleBgColors()
        {
            var colors = Enum.GetValues(typeof(SubtitleBgColorType)).Cast<SubtitleBgColorType>().Select(color => new Item
            {
                Id = color.ToString(),
                Text = GetText(color.ToString())
            });

            return new MvxObservableCollection<Item>(colors);
        }

        private MvxObservableCollection<Item> GetSubtitleFontStyles()
        {
            var styles = Enum.GetValues(typeof(TextTrackFontStyleType)).Cast<TextTrackFontStyleType>().Select(style =>
            {
                var text = style switch
                {
                    TextTrackFontStyleType.Normal => GetText(style.ToString()),
                    TextTrackFontStyleType.Bold => GetText(style.ToString()),
                    TextTrackFontStyleType.BoldItalic => GetText("Bold") + " & " + GetText("Italic"),
                    TextTrackFontStyleType.Italic => GetText(style.ToString()),
                    _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
                };
                return new Item
                {
                    Id = style.ToString(),
                    Text = text
                };
            });

            return new MvxObservableCollection<Item>(styles);
        }

        private MvxObservableCollection<Item> GetSubtitleFontFamilies()
        {
            var families = Enum.GetValues(typeof(TextTrackFontGenericFamilyType)).Cast<TextTrackFontGenericFamilyType>()
                .Select(family =>
                {
                    var text = string.Join(" ", Regex.Split($"{family}", "(?<!^)(?=[A-Z])"));
                    return new Item
                    {
                        Id = family.ToString(),
                        Text = text
                    };
                });

            return new MvxObservableCollection<Item>(families);
        }

        private MvxObservableCollection<Item> GetFontScales()
        {
            var scales = Enum.GetValues(typeof(SubtitleFontScaleType)).Cast<SubtitleFontScaleType>()
                .Select(scale => new Item
                {
                    Id = scale.ToString(),
                    Text = $"{(int)scale} %"
                });

            return new MvxObservableCollection<Item>(scales);
        }

        private async void TriggerSettingsChanged()
        {
            if (!_updatingSettings)
            {
                return;
            }
            var settings = BuildServerAppSettings();
            await _castItHub.UpdateSettings(settings);
        }

        private ServerAppSettings BuildServerAppSettings()
        {
            return new ServerAppSettings
            {
                FFmpegExePath = _fFmpegExePath,
                FFprobeExePath = _ffprobeExePath,
                StartFilesFromTheStart = StartFilesFromTheStart,
                PlayNextFileAutomatically = PlayNextFileAutomatically,
                ForceAudioTranscode = ForceAudioTranscode,
                ForceVideoTranscode = ForceVideoTranscode,
                EnableHardwareAcceleration = EnableHardwareAcceleration,
                VideoScale = (VideoScaleType)Enum.Parse(typeof(VideoScaleType), CurrentVideoScale.Id, true),
                CurrentSubtitleBgColor = (SubtitleBgColorType)Enum.Parse(typeof(SubtitleBgColorType), CurrentSubtitleBgColor.Id, true),
                CurrentSubtitleFgColor = (SubtitleFgColorType)Enum.Parse(typeof(SubtitleFgColorType), CurrentSubtitleFgColor.Id, true),
                CurrentSubtitleFontScale = (SubtitleFontScaleType)Enum.Parse(typeof(SubtitleFontScaleType), CurrentSubtitleFontScale.Id, true),
                CurrentSubtitleFontStyle = (TextTrackFontStyleType)Enum.Parse(typeof(TextTrackFontStyleType), CurrentSubtitleFontStyle.Id, true),
                CurrentSubtitleFontFamily = (TextTrackFontGenericFamilyType)Enum.Parse(typeof(TextTrackFontGenericFamilyType), CurrentSubtitleFontFamily.Id, true),
                SubtitleDelayInSeconds = SubtitleDelay,
                LoadFirstSubtitleFoundAutomatically = LoadFirstSubtitleFoundAutomatically
            };
        }

        private void OnSettingsChange(ServerAppSettings settings)
        {
            _updatingSettings = false;
            StartFilesFromTheStart = settings.StartFilesFromTheStart;
            PlayNextFileAutomatically = settings.PlayNextFileAutomatically;
            ForceVideoTranscode = settings.ForceVideoTranscode;
            ForceAudioTranscode = settings.ForceAudioTranscode;
            CurrentVideoScale = VideoScales.First(v => v.Id == settings.VideoScale.ToString());
            EnableHardwareAcceleration = settings.EnableHardwareAcceleration;
            CurrentSubtitleFgColor = SubtitleFgColors.First(v => v.Id == settings.CurrentSubtitleFgColor.ToString());
            CurrentSubtitleBgColor = SubtitleBgColors.First(v => v.Id == settings.CurrentSubtitleBgColor.ToString());
            CurrentSubtitleFontScale = SubtitleFontScales.First(v => v.Id == settings.CurrentSubtitleFontScale.ToString());
            CurrentSubtitleFontStyle = SubtitleFontStyles.First(v => v.Id == settings.CurrentSubtitleFontStyle.ToString());
            CurrentSubtitleFontFamily = SubtitleFontFamilies.First(v => v.Id == settings.CurrentSubtitleFontFamily.ToString());
            SubtitleDelay = settings.SubtitleDelayInSeconds;
            LoadFirstSubtitleFoundAutomatically = settings.LoadFirstSubtitleFoundAutomatically;

            _fFmpegExePath = settings.FFmpegExePath;
            _ffprobeExePath = settings.FFprobeExePath;
            _updatingSettings = true;

            //These lines must happen AFTER the _updatingSettings
            if (string.IsNullOrWhiteSpace(_fFmpegExePath) || string.IsNullOrWhiteSpace(_ffprobeExePath))
            {
                Messenger.Publish(new ShowDownloadFFmpegDialogMessage(this));
            }
        }
    }
}
