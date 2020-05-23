using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class ShowFileDetailsMessage : MvxMessage
    {
        public bool Show { get; }
        public ShowFileDetailsMessage(object sender, bool show) : base(sender)
        {
            Show = show;
        }
    }
}
