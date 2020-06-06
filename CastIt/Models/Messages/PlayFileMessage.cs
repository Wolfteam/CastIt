using CastIt.ViewModels.Items;
using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class PlayFileMessage : MvxMessage
    {
        public FileItemViewModel File { get; }
        public bool Force { get; }

        public PlayFileMessage(FileItemViewModel sender, bool force = false)
            : base(sender)
        {
            File = sender;
            Force = force;
        }
    }
}
