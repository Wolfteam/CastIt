using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class ShowPlayListTotalDurationMessage : MvxMessage
    {
        public bool Show { get; }
        public ShowPlayListTotalDurationMessage(object sender, bool show) 
            : base(sender)
        {
            Show = show;
        }
    }
}
