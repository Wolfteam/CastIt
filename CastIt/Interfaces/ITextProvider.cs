using CastIt.Domain.Enums;
using MvvmCross.Localization;
using System.Globalization;

namespace CastIt.Interfaces
{
    public interface ITextProvider : IMvxTextProvider
    {
        CultureInfo CurrentCulture { get; }
        string Get(string key);
        string Get(string key, params string[] formatArgs);
        void SetLanguage(AppLanguageType appLanguage, bool notifyAllVms = false);
    }
}
