using System;

namespace CastIt.Cli.Common.Exceptions
{
    public class BaseCliException : Exception
    {
        public BaseCliException(string message) : base(message)
        {
        }

        private BaseCliException() : base()
        {
        }

        private BaseCliException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
