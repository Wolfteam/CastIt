using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class FFmpegInvalidExecutable : BaseAppException
    {
        public FFmpegInvalidExecutable(string ffmpegPath, string ffprobePath) : this(
            $"Either the ffmpeg exe = {ffmpegPath} or ffprobe exe = {ffprobePath} are not valid or were not found",
            AppMessageType.FFmpegExecutableNotFound)
        {
        }

        private FFmpegInvalidExecutable(string message, AppMessageType errorMessageId)
            : base(message, errorMessageId)
        {
        }
    }
}
