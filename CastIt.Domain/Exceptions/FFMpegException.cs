using System;

namespace CastIt.Domain.Exceptions
{
    public class FFmpegException : Exception
    {
        public string Command { get; }

        public FFmpegException() : base()
        {
        }

        public FFmpegException(string message) : base(message)
        {
        }

        public FFmpegException(
            string message,
            Exception innerException) : base(message, innerException)
        {
        }

        public FFmpegException(
            string message,
            string cmd) : this(message)
        {
            Command = cmd;
        }
    }
}
