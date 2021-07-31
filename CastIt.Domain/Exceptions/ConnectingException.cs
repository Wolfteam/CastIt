using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class ConnectingException : BaseAppException
    {
        public ConnectingException(string message, AppMessageType errorMessageId = AppMessageType.ConnectionToDeviceIsStillInProgress)
            : base(message, errorMessageId)
        {
        }
    }
}
