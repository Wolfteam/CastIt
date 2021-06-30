using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class NoDevicesException : BaseAppException
    {
        public NoDevicesException(string message, AppMessageType errorMessageId = AppMessageType.NoDevicesFound)
            : base(message, errorMessageId)
        {
        }
    }
}
