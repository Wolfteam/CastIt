using CastIt.Common;
using CastIt.Common.Enums;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models;
using CastIt.Models.Messages;
using MvvmCross.Commands;
using MvvmCross.Logging;
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
        private Item _currentTheme;
        private Item _currentLanguage;

        private readonly MvxInteraction<string> _changeSelectedAccentColor = new MvxInteraction<string>();
        #endregion     

        #region Properties
        public MvxObservableCollection<Item> Themes
            => GetThemes();
        public Item CurrentTheme
        {
            get
            {
                _currentTheme = Themes.First(l => l.Id == _settingsService.AppTheme.ToString());
                return _currentTheme;
            }
            set
            {
                if (value == null)
                    return;
                var selectedTheme = (AppThemeType)Enum.Parse(typeof(AppThemeType), value.Id, true);
                WindowsUtils.ChangeTheme(selectedTheme, _settingsService.AccentColor);
                _settingsService.AppTheme = selectedTheme;
                SetProperty(ref _currentTheme, value);
            }
        }

        public MvxObservableCollection<Item> Languages
            => GetLanguages();
        public Item CurrentLanguage
        {
            get
            {
                _currentLanguage = Languages.First(l => l.Id == _settingsService.Language.ToString());
                return _currentLanguage;
            }
            set
            {
                if (value == null)
                    return;
                var selectedLang = (AppLanguageType)Enum.Parse(typeof(AppLanguageType), value.Id, true);
                _settingsService.Language = selectedLang;
                TextProvider.SetLanguage(selectedLang, true);
                SetProperty(ref _currentLanguage, value);
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
        #endregion

        #region Commands
        public IMvxCommand<string> AccentColorChangedCommand { get; private set; }
        #endregion

        #region Interactos
        public IMvxInteraction<string> ChangeSelectedAccentColor
            => _changeSelectedAccentColor;
        #endregion

        public SettingsViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            IAppSettingsService settingsService)
            : base(textProvider, messenger, logger.GetLogFor<SettingsViewModel>())
        {
            _settingsService = settingsService;
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
    }
}
