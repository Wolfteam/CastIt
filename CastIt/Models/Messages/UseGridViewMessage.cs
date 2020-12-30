using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class UseGridViewMessage : MvxMessage
    {
        public bool UseAGridView { get; set; }

        public UseGridViewMessage(object sender, bool useAGridView) : base(sender)
        {
            UseAGridView = useAGridView;
        }
    }
}
