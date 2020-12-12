using CastIt.Domain.Enums;
using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class AppLanguageChangedMessage : MvxMessage
    {
        public AppLanguageType NewLanguage { get; }
        public AppLanguageChangedMessage(object sender, AppLanguageType appLanguage) : base(sender)
        {
            NewLanguage = appLanguage;
        }
    }
}
