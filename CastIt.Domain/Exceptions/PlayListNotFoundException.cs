using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class PlayListNotFoundException : BaseAppException
    {
        public PlayListNotFoundException(string message, AppMessageType errorMessageId = AppMessageType.PlayListNotFound)
            : base(message, errorMessageId)
        {
        }

        public PlayListNotFoundException(string name, long id, AppMessageType errorMessageId = AppMessageType.PlayListNotFound)
            : base($"Resource = {name} associated to id = {id} was not found.", errorMessageId)
        {
        }
    }
}
