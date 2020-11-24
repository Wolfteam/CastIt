using CastIt.Domain.Enums;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.ResxLocalization;
using System.Globalization;
using System.Resources;

namespace CastIt.Resources
{
    public class ResxTextProvider : MvxResxTextProvider, ITextProvider
    {
        private readonly IMvxMessenger _messenger;
        public CultureInfo CurrentCulture
            => CurrentLanguage;

        public ResxTextProvider(
            ResourceManager resourceManager,
            IMvxMessenger messenger)
            : base(resourceManager)
        {
            _messenger = messenger;
        }

        public string Get(string key)
        {
            return GetText(string.Empty, string.Empty, key);
        }

        public string Get(string key, params string[] formatArgs)
        {
            return GetText(string.Empty, string.Empty, key, formatArgs);
        }

        public void SetLanguage(AppLanguageType appLanguage, bool notifyAllVms = false)
        {
            string lang = appLanguage == AppLanguageType.English
                ? "en"
                : "es";
            CurrentLanguage = new CultureInfo(lang);
            //let all ViewModels that are active know, that the culture has changed
            if (notifyAllVms)
                _messenger.Publish(new AppLanguageChangedMessage(this, appLanguage));
        }
    }

}
