using CastIt.ViewModels.Items;
using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class LoopFileMessage : MvxMessage
    {
        public FileItemViewModel File { get; set; }
        public LoopFileMessage(object sender) : base(sender)
        {
            File = sender as FileItemViewModel;
        }
    }
}
