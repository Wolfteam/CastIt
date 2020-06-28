using System;

namespace CastIt.Common.Exceptions
{
    public class FFMpegException : Exception
    {
        public string Command { get; }

        public FFMpegException() : base()
        {
        }

        public FFMpegException(string message) : base(message)
        {
        }

        public FFMpegException(
            string message,
            Exception innerException) : base(message, innerException)
        {
        }

        public FFMpegException(
            string message,
            string cmd) : this(message)
        {
            Command = cmd;
        }
    }
}
