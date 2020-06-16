using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class SnackbarMessage : MvxMessage
    {
        public string Message { get; }
        public SnackbarMessage(object sender, string msg) : base(sender)
        {
            Message = msg;
        }
    }
}
