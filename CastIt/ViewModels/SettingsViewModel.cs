using CastIt.Common;
using CastIt.Common.Enums;
using CastIt.Common.Utils;
using CastIt.GoogleCast.Enums;
using CastIt.Interfaces;
using CastIt.Models;
using CastIt.Models.Messages;
using CastIt.ViewModels.Dialogs;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CastIt.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Members
        private readonly IAppSettingsService _settingsService;
        private readonly IMvxNavigationService _navigationService;
        private readonly IAppWebServer _appWebServer;

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
            get => _settingsService.StartFilesFromTheStart;
            set
            {
                _settingsService.StartFilesFromTheStart = value;
                RaisePropertyChanged(() => StartFilesFromTheStart);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public bool PlayNextFileAutomatically
        {
            get => _settingsService.PlayNextFileAutomatically;
            set
            {
                _settingsService.PlayNextFileAutomatically = value;
                RaisePropertyChanged(() => PlayNextFileAutomatically);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public bool ForceVideoTranscode
        {
            get => _settingsService.ForceVideoTranscode;
            set
            {
                _settingsService.ForceVideoTranscode = value;
                RaisePropertyChanged(() => ForceVideoTranscode);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public bool ForceAudioTranscode
        {
            get => _settingsService.ForceAudioTranscode;
            set
            {
                _settingsService.ForceAudioTranscode = value;
                RaisePropertyChanged(() => ForceAudioTranscode);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public MvxObservableCollection<Item> VideoScales
            => GetVideoScales();

        public Item CurrentVideoScale
        {
            get => VideoScales.First(l => l.Id == _settingsService.VideoScale.ToString());
            set
            {
                if (value == null)
                    return;
                _settingsService.VideoScale = (VideoScaleType)Enum.Parse(typeof(VideoScaleType), value.Id, true);
                RaisePropertyChanged(() => CurrentVideoScale);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public bool EnableHardwareAcceleration
        {
            get => _settingsService.EnableHardwareAcceleration;
            set
            {
                _settingsService.EnableHardwareAcceleration = value;
                RaisePropertyChanged(() => EnableHardwareAcceleration);
                _appWebServer.OnAppSettingsChanged?.Invoke();
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

        #region Subtitles
        public MvxObservableCollection<Item> SubtitleFgColors
            => GetSubtitleFgColors();
        public MvxObservableCollection<Item> SubtitleBgColors
            => GetSubtitleBgColors();
        public MvxObservableCollection<Item> SubtitleFontStyles
            => GetSubtitleFontStyles();
        public MvxObservableCollection<Item> SubtitleFontFamilies
            => GetSubtitleFontFamilies();
        public MvxObservableCollection<Item> SubtitleFontScales
            => GetFontScales();

        public Item CurrentSubtitleFgColor
        {
            get => SubtitleFgColors.First(l => l.Id == _settingsService.CurrentSubtitleFgColor.ToString());
            set
            {
                if (value == null)
                    return;
                _settingsService.CurrentSubtitleFgColor = (SubtitleFgColorType)Enum.Parse(typeof(SubtitleFgColorType), value.Id, true);
                RaisePropertyChanged(() => CurrentSubtitleFgColor);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public Item CurrentSubtitleBgColor
        {
            get => SubtitleBgColors.First(l => l.Id == _settingsService.CurrentSubtitleBgColor.ToString());
            set
            {
                if (value == null)
                    return;
                _settingsService.CurrentSubtitleBgColor = (SubtitleBgColorType)Enum.Parse(typeof(SubtitleBgColorType), value.Id, true);
                RaisePropertyChanged(() => CurrentSubtitleBgColor);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public Item CurrentSubtitleFontScale
        {
            get => SubtitleFontScales.First(l => l.Id == _settingsService.CurrentSubtitleFontScale.ToString());
            set
            {
                if (value == null)
                    return;
                _settingsService.CurrentSubtitleFontScale = (SubtitleFontScaleType)Enum.Parse(typeof(SubtitleFontScaleType), value.Id, true);
                RaisePropertyChanged(() => CurrentSubtitleFontScale);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public Item CurrentSubtitleFontStyle
        {
            get => SubtitleFontStyles.First(l => l.Id == _settingsService.CurrentSubtitleFontStyle.ToString());
            set
            {
                if (value == null)
                    return;
                _settingsService.CurrentSubtitleFontStyle = (TextTrackFontStyleType)Enum.Parse(typeof(TextTrackFontStyleType), value.Id, true);
                RaisePropertyChanged(() => CurrentSubtitleFontStyle);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public Item CurrentSubtitleFontFamily
        {
            get => SubtitleFontFamilies.First(l => l.Id == _settingsService.CurrentSubtitleFontFamily.ToString());
            set
            {
                if (value == null)
                    return;
                _settingsService.CurrentSubtitleFontFamily = (TextTrackFontGenericFamilyType)Enum.Parse(typeof(TextTrackFontGenericFamilyType), value.Id, true);
                RaisePropertyChanged(() => CurrentSubtitleFontFamily);
                _appWebServer.OnAppSettingsChanged?.Invoke();
            }
        }

        public string SubtitleDelayText
            => $"{GetText("SubtitleDelay")} ({GetText("XSeconds", $"{(SubtitleDelay == 0 ? 0 : SubtitleDelay)}")})";
        public double SubtitleDelay
        {
            get => _settingsService.SubtitleDelayInSeconds;
            set
            {
                _settingsService.SubtitleDelayInSeconds = Math.Round(value, 1);
                RaisePropertyChanged(() => SubtitleDelay);
                RaisePropertyChanged(() => SubtitleDelayText);
            }
        }

        public bool LoadFirstSubtitleFoundAutomatically
        {
            get => _settingsService.LoadFirstSubtitleFoundAutomatically;
            set
            {
                _settingsService.LoadFirstSubtitleFoundAutomatically = value;
                RaisePropertyChanged(() => LoadFirstSubtitleFoundAutomatically);
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
            IMvxLogProvider logger,
            IAppSettingsService settingsService,
            IMvxNavigationService navigationService,
            IAppWebServer appWebServer)
            : base(textProvider, messenger, logger.GetLogFor<SettingsViewModel>())
        {
            _settingsService = settingsService;
            _navigationService = navigationService;
            _appWebServer = appWebServer;
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

            SubscriptionTokens.AddRange(new[]
            {
                Messenger.Subscribe<SettingsExternallyUpdatedMessage>(SettingsExternallyUpdated)
            });
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

        private void SettingsExternallyUpdated(SettingsExternallyUpdatedMessage msg)
        {
            StartFilesFromTheStart = msg.StartFilesFromTheStart;
            PlayNextFileAutomatically = msg.PlayNextFileAutomatically;
            ForceVideoTranscode = msg.ForceVideoTranscode;
            ForceAudioTranscode = msg.ForceAudioTranscode;
            CurrentVideoScale = VideoScales.FirstOrDefault(v => v.Id == msg.VideoScale.ToString());
            EnableHardwareAcceleration = msg.EnableHardwareAcceleration;
        }
    }
}
