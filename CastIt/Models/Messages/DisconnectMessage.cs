using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class DisconnectMessage : MvxMessage
    {
        public DisconnectMessage(object sender) : base(sender)
        {
        }
    }
}
