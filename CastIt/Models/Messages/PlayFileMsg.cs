using CastIt.ViewModels.Items;
using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class PlayFileMsg : MvxMessage
    {
        public FileItemViewModel File { get; private set; }

        public PlayFileMsg(FileItemViewModel sender) 
            : base(sender)
        {
            File = sender;
        }
    }
}
