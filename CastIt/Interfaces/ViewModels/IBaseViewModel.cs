using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;

namespace CastIt.Interfaces.ViewModels
{
    public interface IBaseViewModel
    {
        ITextProvider TextProvider { get; }
        IMvxMessenger Messenger { get; }
        IMvxLog Logger { get; }
        string this[string key] { get; }

        string GetText(string key);
        string GetText(string key, params string[] args);
    }
}
