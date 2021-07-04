using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class FfmpegPathChangedMessage : MvxMessage
    {
        public string FolderPath { get; }
        public FfmpegPathChangedMessage(object sender, string folderPath) : base(sender)
        {
            FolderPath = folderPath;
        }
    }
}
