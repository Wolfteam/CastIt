using System;

namespace CastIt.Domain.Exceptions
{
    public class NoDevicesException : Exception
    {
        public NoDevicesException() : base()
        {
        }

        public NoDevicesException(string message) : base(message)
        {
        }

        public NoDevicesException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
