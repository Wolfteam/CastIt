using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class FFmpegException : BaseAppException
    {
        public string Command { get; }

        public FFmpegException(
            string message,
            string cmd,
            AppMessageType errorMessageId = AppMessageType.FFmpegError)
            : this(message, errorMessageId)
        {
            Command = cmd;
        }

        private FFmpegException(string message, AppMessageType errorMessageId) : base(message, errorMessageId)
        {
        }
    }
}
