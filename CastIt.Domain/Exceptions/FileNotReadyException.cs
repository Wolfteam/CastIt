using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class FileNotReadyException : BaseAppException
    {
        public FileNotReadyException(string message, AppMessageType errorMessageId = AppMessageType.OneOrMoreFilesAreNotReadyYet)
            : base(message, errorMessageId)
        {
        }
    }
}
