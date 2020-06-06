using CastIt.GoogleCast.Extensions;
using CastIt.GoogleCast.Interfaces.Messages;
using System;

namespace CastIt.GoogleCast.Messages.Base
{
    public abstract class Message : IMessage
    {
        public string Type { get; set; }

        protected Message()
        {
            Type = GetMessageType(GetType());
        }

        public static string GetMessageType(Type type)
        {
            var typeName = type.Name;
            var t = typeName.Substring(0, typeName.LastIndexOf(nameof(Message)));
            return t.ToUnderscoreUpperInvariant();
        }
    }
}
