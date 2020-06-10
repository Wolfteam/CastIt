using CastIt.Common;
using CastIt.Common.Enums;
using CastIt.Common.Utils;
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

namespace CastIt.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Members
        private readonly IAppSettingsService _settingsService;
        private readonly IMvxNavigationService _navigationService;

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
            }
        }

        public bool PlayNextFileAutomatically
        {
            get => _settingsService.PlayNextFileAutomatically;
            set
            {
                _settingsService.PlayNextFileAutomatically = value;
                RaisePropertyChanged(() => PlayNextFileAutomatically);
            }
        }

        public bool ForceVideoTranscode
        {
            get => _settingsService.ForceVideoTranscode;
            set
            {
                _settingsService.ForceVideoTranscode = value;
                RaisePropertyChanged(() => ForceVideoTranscode);
            }
        }

        public bool ForceAudioTranscode
        {
            get => _settingsService.ForceAudioTranscode;
            set
            {
                _settingsService.ForceAudioTranscode = value;
                RaisePropertyChanged(() => ForceAudioTranscode);
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
            }
        }

        public bool EnableHardwareAcceleration
        {
            get => _settingsService.EnableHardwareAcceleration;
            set
            {
                _settingsService.EnableHardwareAcceleration = value;
                RaisePropertyChanged(() => EnableHardwareAcceleration);
            }
        }
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
            IMvxNavigationService navigationService)
            : base(textProvider, messenger, logger.GetLogFor<SettingsViewModel>())
        {
            _settingsService = settingsService;
            _navigationService = navigationService;
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
    }
}
