using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class FileNotSupportedException : BaseAppException
    {
        public FileNotSupportedException(string message, AppMessageType errorMessageId = AppMessageType.FileNotSupported)
            : base(message, errorMessageId)
        {
        }
    }
}
