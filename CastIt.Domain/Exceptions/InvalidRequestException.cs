using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class InvalidRequestException : BaseAppException
    {
        public InvalidRequestException(string message, AppMessageType errorMessageId = AppMessageType.InvalidRequest)
            : base(message, errorMessageId)
        {
        }
    }
}
