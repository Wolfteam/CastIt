using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class FileNotFoundException : BaseAppException
    {
        public FileNotFoundException(string message, AppMessageType errorMessageId = AppMessageType.FileNotFound)
            : base(message, errorMessageId)
        {
        }

        public FileNotFoundException(string name, long id, AppMessageType errorMessageId = AppMessageType.FileNotFound)
            : base($"Resource = {name} associated to id = {id} was not found.", errorMessageId)
        {
        }
    }
}
