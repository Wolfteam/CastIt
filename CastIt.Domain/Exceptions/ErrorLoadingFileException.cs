using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class ErrorLoadingFileException : BaseAppException
    {
        public ErrorLoadingFileException(string message, AppMessageType errorMessageId = AppMessageType.UnknownErrorLoadingFile)
            : base(message, errorMessageId)
        {
        }
    }
}
