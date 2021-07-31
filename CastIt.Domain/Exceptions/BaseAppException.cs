using CastIt.Domain.Enums;
using System;

namespace CastIt.Domain.Exceptions
{
    public abstract class BaseAppException : Exception
    {
        public AppMessageType ErrorMessageId { get; }

        protected BaseAppException(string message, AppMessageType errorMessageId)
            : base(message)
        {
            ErrorMessageId = errorMessageId;
        }

        private BaseAppException()
            : base()
        {
            ErrorMessageId = AppMessageType.UnknownErrorOccurred;
        }

        private BaseAppException(string message)
            : base(message)
        {
            ErrorMessageId = AppMessageType.UnknownErrorOccurred;
        }

        private BaseAppException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
