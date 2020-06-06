using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Messages.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CastIt.GoogleCast.Messages
{
    public class SupportedMessages : Dictionary<string, Type>
    {
        public SupportedMessages()
        {
            AddMessageTypes(Assembly.GetExecutingAssembly());
        }

        public void AddMessageTypes(Assembly assembly)
        {
            var messageInterfaceType = typeof(IMessage);
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && messageInterfaceType.IsAssignableFrom(t))
                .ToList();
            foreach (var type in types)
            {
                if (!ContainsKey(Message.GetMessageType(type)))
                {
                    Add(Message.GetMessageType(type), type);
                }
            }
        }
    }
}
