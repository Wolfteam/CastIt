using CastIt.GoogleCast.Interfaces.Messages;
using System;
using System.Threading;

namespace CastIt.GoogleCast.Messages.Base
{
    public class MessageWithId : Message, IMessageWithId
    {
        private static int _id = new Random().Next();
        private int? _requestId;

        public bool HasRequestId
            => _requestId.HasValue;

        public int RequestId
        {
            get { return (int)(int?)(_requestId ??= Interlocked.Increment(ref _id)); }
            set { _requestId = value; }
        }
    }
}
