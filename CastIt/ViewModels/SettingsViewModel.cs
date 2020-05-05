using CastIt.Common;
using CastIt.Common.Enums;
using CastIt.Interfaces;
using CastIt.Models;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Members
        private readonly IAppSettingsService _settingsService;
        private Item _currentTheme;
        private Item _currentLanguage;

        private MvxInteraction<string> _changeSelectedAccentColor = new MvxInteraction<string>();
        #endregion     

        #region Properties
        public MvxObservableCollection<Item> Themes { get; private set; }
        public Item CurrentTheme
        {
            get
            {
                var theme = Themes.First(l => l.Id == _settingsService.AppTheme.ToString());
                _currentTheme = theme;
                return _currentTheme;
            }
            set
            {
                var selectedTheme = (AppThemeType)Enum.Parse(typeof(AppThemeType), value.Id, true);
                _settingsService.AppTheme = selectedTheme;
                SetProperty(ref _currentTheme, value);
            }
        }

        public MvxObservableCollection<Item> Languages { get; private set; }
        public Item CurrentLanguage
        {
            get
            {
                var item = Languages.First(l => l.Id == _settingsService.Language.ToString());
                _currentLanguage = item;
                return _currentLanguage;
            }
            set
            {
                var selectedLang = (AppLanguageType)Enum.Parse(typeof(AppLanguageType), value.Id, true);
                _settingsService.Language = selectedLang;
                SetProperty(ref _currentLanguage, value);
            }
        }

        public List<string> AccentColors
                    => AppConstants.AppAccentColors.ToList();

        public string CurrentAccentColor
            => _settingsService.AccentColor;
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
            var themes = Enum.GetValues(typeof(AppThemeType)).Cast<AppThemeType>().Select(theme => new Item
            {
                Id = theme.ToString(),
                Text = theme.ToString()
            });

            Themes = new MvxObservableCollection<Item>(themes);

            var languages = Enum.GetValues(typeof(AppLanguageType)).Cast<AppLanguageType>().Select(lang => new Item
            {
                Id = lang.ToString(),
                Text = lang.ToString()
            });

            Languages = new MvxObservableCollection<Item>(languages);
        }

        public override void SetCommands()
        {
            base.SetCommands();
            AccentColorChangedCommand = new MvxCommand<string>((hexColor) =>
            {
                _settingsService.AccentColor = hexColor;
                _changeSelectedAccentColor.Raise(hexColor);
            });
        }
    }
}
