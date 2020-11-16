using System;

namespace CastIt.Domain.Exceptions
{
    public class ConnectingException : Exception
    {
        public ConnectingException() : base()
        {
        }

        public ConnectingException(string message) : base(message)
        {
        }

        public ConnectingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
