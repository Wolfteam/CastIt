using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class IsBusyMessage : MvxMessage
    {
        public bool IsBusy { get; }

        public IsBusyMessage(object sender, bool isBusy) : base(sender)
        {
            IsBusy = isBusy;
        }
    }
}
