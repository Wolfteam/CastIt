using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class ManualDisconnectMessage : MvxMessage
    {
        public ManualDisconnectMessage(object sender) : base(sender)
        {
        }
    }
}
