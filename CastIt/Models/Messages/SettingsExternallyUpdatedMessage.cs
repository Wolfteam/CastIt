using CastIt.Common.Enums;
using MvvmCross.Plugin.Messenger;

namespace CastIt.Models.Messages
{
    public class SettingsExternallyUpdatedMessage : MvxMessage
    {
        public bool StartFilesFromTheStart { get; }
        public bool PlayNextFileAutomatically { get; }
        public bool ForceVideoTranscode { get; }
        public bool ForceAudioTranscode { get; }
        public VideoScaleType VideoScale { get; }
        public bool EnableHardwareAcceleration { get; }

        public SettingsExternallyUpdatedMessage(
            object sender,
            bool startFilesFromTheStart,
            bool playNextFileAutomatically,
            bool forceVideoTranscode,
            bool forceAudioTranscode,
            VideoScaleType videoScale,
            bool enableHardwareAcceleration) : base(sender)
        {
            StartFilesFromTheStart = startFilesFromTheStart;
            PlayNextFileAutomatically = playNextFileAutomatically;
            ForceVideoTranscode = forceVideoTranscode;
            ForceAudioTranscode = forceAudioTranscode;
            VideoScale = videoScale;
            EnableHardwareAcceleration = enableHardwareAcceleration;
        }
    }
}
