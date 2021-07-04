using CastIt.Domain.Enums;

namespace CastIt.Domain.Exceptions
{
    public class UrlCouldNotBeParsedException : BaseAppException
    {
        public UrlCouldNotBeParsedException(string url, AppMessageType errorMessageId = AppMessageType.UrlCouldntBeParsed)
            : base($"The {url} could not be parsed", errorMessageId)
        {
        }
    }
}
